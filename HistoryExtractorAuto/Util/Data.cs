using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace HistoryExtractorAuto.Util
{
    class Data
    {
        //Variables Globales con los datos de la instancia de PRTG.
        #region Globals

        private static readonly Conexion cnn = new Conexion();
        private static readonly string usr = "appauto";
        private static readonly string pssd = "A43f7e2d41f30d4790333c4843e97e50";
        //private static readonly string URLInStringFormat = "https://prtg01.e-global.com.co/api/";
        
        
        #endregion

        //Metodo para omitir los certificados SSL
        #region SSL Certificade 

        public static HttpClientHandler IgnoreBadCertificates()
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (
                sender, cert, chain, sslPolicyErrors) =>
            { return true; };

            return clientHandler;
        }
        public int _fallidos;
        public int _Fails
        {
            get => _fallidos;
            set
            {
                _fallidos = value;
            }
        }

        #endregion

        /// <summary>
        /// Metodo principal para extraer la disponibilidad de los dispositivos.
        /// </summary>
        /// <param name="indate"></param>
        /// <param name="querySearch"></param>
        /// <param name="querySave"></param>
        /// <param name="querySaveDetails"></param>
        public void ExtractAvailability(string indate, string enddate, string querySearch, string querySave, string querySaveDetails, StringBuilder sb, string URLInStringFormat, string inhour00, string endhour00, string inhour05, string endhour05)
        {            
            using (cnn.connection)
            {
                using (var cmd = new SqlCommand(querySearch)) //Realiza consulta en BD de los sensores PING para todos los dispositivos.          
                {
                    try
                    {
                        cnn.Connect();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = cnn.connection;
                        StringBuilder parceSb = new StringBuilder();
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                try
                                {
                                    int id = Convert.ToInt32(dr["ID_Sensor"]);
                                    int idDev = Convert.ToInt32(dr["ID_Device"]);
                                    string nameDev = Convert.ToString(dr["Name"]);
                                    DeviceData Element = ApiAvailability(id, indate, URLInStringFormat, inhour00, endhour00);
                                    if (Element != null)
                                    {
                                        Element.idSensor = id;
                                        Element.idDevice = idDev;
                                        Element.nameDevice = nameDev;
                                        parceSb.Append("Reading: ").Append(id).Append(", ").Append(idDev).Append(", ").Append(nameDev);
                                        Console.WriteLine(parceSb.ToString(), 2);
                                        parceSb.Clear();
                                        SaveAvailability(Element, indate, querySave);
                                        ApiAvailabilityDetails(Element, indate, enddate, querySaveDetails, URLInStringFormat, inhour05, endhour05);
                                    }
                                    else
                                    {
                                        ++_fallidos;
                                        sb.Append("Sensor id: ").Append(id).Append(", Device id: ").Append(idDev).Append("\n"); ;
                                        Console.WriteLine(sb.ToString(), 1);
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("ExtractAvailability " + ex.Message.ToString(), 1);
                                    ++_fallidos;
                                    continue;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error de ejecucion - Funcion: ExtractAvailability. " + ex.Message, 1);
                        throw;
                    }
                    finally {
                        cmd.Connection.Close();
                        cmd.Connection.Dispose();
                    }
                }
            }
        }
        
        /// <summary>
        /// Metodo para consumir Api de Disponibilidad y extraer datos de archivo Xml con tipo CDATA.
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="indate"></param>
        /// <returns></returns>
        private static DeviceData ApiAvailability(int sensor, string indate,  string URLInStringFormat, string inhour, string endhour)
        {
            var clientHandler = IgnoreBadCertificates();
            DeviceData Element = new DeviceData();
            string result = "";
            StringBuilder parceSb = new StringBuilder();
            using (var client = new HttpClient(clientHandler))
            {
                client.BaseAddress = new Uri(URLInStringFormat, UriKind.Absolute);
                try
                {
                    var response = client.GetAsync("historicdataupt.xml?username="
                    + usr + "&password=" + pssd + "&id=" + sensor + "&sdate="
                    + indate + inhour +"&edate=" + indate + endhour +"&avg=3600&pctavg=300&pctshow=false&pct=95&pctmode=true");
                    result = response.Result.Content.ReadAsStringAsync().Result;
                    
                    Console.WriteLine("Sending http request...");                 

                    XElement XTemp = XElement.Parse(result);
                    var GetCDATA = from element in XTemp.DescendantNodes()
                                   where element.NodeType == XmlNodeType.CDATA
                                   select element.Parent.Value.Trim();
                    string uptimePercent = GetCDATA.ToList<string>()[0].ToString();
                    if (uptimePercent != null)
                    {
                        
                        Element.dnTime = GetCDATA.ToList<string>()[1].ToString();

                        char[] charsToTrim = { ' ', '%' };
                        uptimePercent = uptimePercent.Trim(charsToTrim).Replace(",", ".").Replace("?", "");
                        Element.uptPercent = uptimePercent;
                        return Element;
                    }
                    else
                    {
                        parceSb.Append("uptimePercent: ").Append(uptimePercent).Append('\n');
                        Console.WriteLine(parceSb.ToString(), 1);
                        return null;
                    }                                            
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ApiAvailability " + ex.Message.ToString(), 1);                    
                    //Console.WriteLine($"ID sensor fallido = {sensor} Resultado de la API = {result}");
                    //var notify = new Notify();
                    ////notify.SendMail(sensor, result, ex);               
                    throw;
                }                
            }
        }

        /// <summary>
        /// Metodo para grabar en la base de datos la disponibilidad.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="indate"></param>
        /// <param name="querySaveDev"></param>
        public static void SaveAvailability(DeviceData data, string indate, string querySaveDev)
        {
            using (var cmd = new SqlCommand(querySaveDev))
            {
                try
                {
                    cnn.Connect();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdSensor", data.idSensor);
                    cmd.Parameters.AddWithValue("@NameDevice", data.nameDevice);
                    cmd.Parameters.AddWithValue("@IdDevice", data.idDevice);
                    cmd.Parameters.AddWithValue("@UptPercent", data.uptPercent);
                    cmd.Parameters.AddWithValue("@DnTime", data.dnTime);
                    cmd.Parameters.AddWithValue("@Indate", indate);
                    cmd.Connection = cnn.connection;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
            }
        }

        /// <summary>
        /// Metodo para consumir Api detalles de Disponibilidad y extraer datos de archivo Xml con tipo Node.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="indate"></param>
        /// <param name="saveDetails"></param>
        public static void ApiAvailabilityDetails(DeviceData data, string indate, string enddate, string saveDetails,  string URLInStringFormat, string inhour, string endhour)
        {
            var clientHandler = IgnoreBadCertificates();
            StringBuilder parseSb = new StringBuilder();
            using (var client = new HttpClient(clientHandler))
            {
                client.BaseAddress = new Uri(URLInStringFormat, UriKind.Absolute);
                var response = client.GetAsync("historicdata.xml?username="
                    + usr + "&password=" + pssd + "&id=" + data.idSensor + "&sdate="
                    + indate + inhour +"&edate=" + enddate
                    + endhour + "&avg=3600&content=statehistory&sortby=datetime&columns=status,datetime&filter_status=5");
                var result = response.Result.Content.ReadAsStringAsync();
                result.Wait();

                Console.WriteLine("Sending http request: " + result.Result);
                //Console.WriteLine("Sendin http request: " + result.Status);
                try
                {
                    XElement rootNode = XElement.Parse(result.Result);
                    List<string> dateList = new List<string>();
                    if (rootNode != null)
                    {
                        foreach (XElement elementItem in rootNode.Descendants("item"))
                        {
                            XElement statusNode = elementItem.Element("status");
                            string statusValue = statusNode.Value;

                            if (statusValue.Contains("Down"))
                            {
                                string dateTimeValue = elementItem.Element("datetime").Value;
                                dateList.Add(dateTimeValue);
                            }
                        }
                        foreach (string dateDowntime in dateList)
                        {
                            using (cnn.connection)
                            {
                                using (var cmd = new SqlCommand(saveDetails))
                                {
                                    try
                                    {
                                        cnn.Connect();
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@IdSensor", data.idSensor);
                                        cmd.Parameters.AddWithValue("@DateDowntime", dateDowntime);
                                        cmd.Parameters.AddWithValue("@Indate", indate);
                                        cmd.Connection = cnn.connection;
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (Exception)
                                    {

                                        throw;
                                    }
                                    finally
                                    {
                                        cmd.Connection.Close();
                                        cmd.Connection.Dispose();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        parseSb.Append("rootNode: ").Append(rootNode).Append("\n");
                        Console.WriteLine(parseSb.ToString(), 1);
                        parseSb.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ApiAvailabilityDetails " + ex.Message, 1);
                    throw;
                }
            }            
        }

        /// <summary>
        /// Metodo para extraer la capacidad de los dispositivos (CPU, RAM, Disco C)
        /// </summary>
        /// <param name="indate"></param>
        /// <param name="querySearch"></param>
        /// <param name="querySave"></param>
        public void ExtractCapacity(string indate, string querySearch, string querySave, StringBuilder sb,  int fallidos, string URLInStringFormat, string inhour, string endhour)
        {
            StringBuilder parceSb = new StringBuilder();
            using (cnn.connection)
            {
                using (var cmd = new SqlCommand(querySearch))
                {
                    cnn.Connect();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = cnn.connection;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            try
                            {
                                int id = Convert.ToInt32(dr["ID_Sensor"]);
                                int idDev = Convert.ToInt32(dr["ID_Device"]);
                                string nameSen = Convert.ToString(dr["NameSensor"]);
                                string nameDev = Convert.ToString(dr["NameDevice"]);

                                DeviceData Element = ApiCapacity(id, indate,  URLInStringFormat, inhour, endhour);
                                if (Element != null)
                                {
                                    Element.idSensorAvg = id;
                                    Element.idDevice = idDev;
                                    Element.nameDevice = nameDev;
                                    parceSb.Append("Reading: ").Append(id).Append(", ").Append(idDev).Append(", ").Append(nameDev).Append(", ").Append(nameSen);
                                    Console.WriteLine(parceSb.ToString(), 2);
                                    parceSb.Clear();
                                    if (nameSen == "CPU Load")
                                    {
                                        nameSen = "CPU";
                                    }
                                    else if (nameSen == "Disk Free")
                                    {
                                        nameSen = "Disk C";
                                    }

                                    Element.nameSensorAvg = nameSen;

                                    SaveCapacity(Element, indate, querySave);
                                }
                                else
                                {
                                    ++fallidos;                                    
                                    sb.Append("Sensor id: ").Append(id).Append(", Device id").Append(idDev).Append("\n");                          
                                    Console.WriteLine(sb.ToString(), 1);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("ExtractCapacity " + ex.Message, 1);
                                continue;
                            }
                                
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Metodo para consumir Api de Capacidad y extraer datos de archivo Xml con tipo CDATA.
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="indate"></param>
        /// <returns></returns>
        private static DeviceData ApiCapacity(int sensor, string indate,  string URLInStringFormat, string inhour, string endhour)
        {
            var clientHandler = IgnoreBadCertificates();
            using (var client = new HttpClient(clientHandler))
            {
                client.BaseAddress = new Uri(URLInStringFormat, UriKind.Absolute);
                var response = client.GetAsync("historicdataavg.xml?username="
                    + usr + "&password=" + pssd + "&id=" + sensor + "&sdate="
                    + indate + inhour + "&edate=" + indate + endhour + "&avg=3600&pctavg=300&pctshow=false&pct=95&pctmode=true");
                var result = response.Result.Content.ReadAsStringAsync();
                result.Wait();

                Console.WriteLine("Sending http request: " + result.Result);             

                XElement XTemp = XElement.Parse(result.Result);
                var GetCDATA = from element in XTemp.DescendantNodes()
                               where element.NodeType == System.Xml.XmlNodeType.CDATA
                               select element.Parent.Value.Trim();

                DeviceData Element = new DeviceData();                
                try
                {
                    string averagePercent = GetCDATA.ToList<string>()[0].ToString();
                    char[] charsToTrim = { ' ', '%' };
                    averagePercent = averagePercent.Trim(charsToTrim).Replace("?", "").Replace("No data", "");

                    string str = "0.";
                    string str2 = "1.";

                    if (!String.IsNullOrEmpty(averagePercent))
                    {
                        if (averagePercent.Contains(str))
                        {
                            averagePercent = averagePercent.Remove(averagePercent.
                            LastIndexOf("0.", StringComparison.InvariantCultureIgnoreCase), 1);
                        }
                        else if (averagePercent.Contains(str2))
                        {
                            averagePercent = averagePercent.Remove(averagePercent.
                            LastIndexOf("1.", StringComparison.InvariantCultureIgnoreCase), 1);
                        }

                        Element.avgPercent = averagePercent;
                    }

                    return Element;
                }
                catch (System.ArgumentOutOfRangeException e)
                {
                    Console.WriteLine("Error ApiCapacity: " + e);
                    return null;
                }                
            }
        }

        /// <summary>
        /// Metodo para grabar la capacidad en la base de datos.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="indate"></param>
        /// <param name="querySaveCapa"></param>
        public static void SaveCapacity(DeviceData data, string indate, string querySaveCapa)
        {
            using (var cmd = new SqlCommand(querySaveCapa))
            {
                try
                {
                    cnn.Connect();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdSensorAvg", data.idSensorAvg);
                    cmd.Parameters.AddWithValue("@NameSensorAvg", data.nameSensorAvg);
                    cmd.Parameters.AddWithValue("@AvgPercent", data.avgPercent);
                    cmd.Parameters.AddWithValue("@Indate", indate);
                    cmd.Parameters.AddWithValue("@IdDevice", data.idDevice);
                    cmd.Parameters.AddWithValue("@NameDevice", data.nameDevice);
                    cmd.Connection = cnn.connection;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
            }
        }

        /// <summary>
        /// Metodo para consultar en BD de los datos que se agregaron en la ejecucion.
        /// </summary>
        /// <param name="indate"></param>
        public void CountData(string indate, StringBuilder sb,  string client, int fallidos)
        {
            using (cnn.connection)
            {
                using (var cmd = new SqlCommand("SP_CountData"))
                {
                    try
                    {
                        cnn.Connect();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Indate", indate);
                        cmd.Connection = cnn.connection;

                        int DevicesAvailability, DevicesCapacity, DevicesDetails, ServicesAvailability, ServicesDetails;

                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                DevicesAvailability = Convert.ToInt32(dr["DevicesAvailability"]);
                                DevicesDetails = Convert.ToInt32(dr["DevicesDetails"]);
                                DevicesCapacity = Convert.ToInt32(dr["DevicesCapacity"]);
                                ServicesAvailability = Convert.ToInt32(dr["ServicesAvailability"]);
                                ServicesDetails = Convert.ToInt32(dr["ServicesDetails"]);
                                Console.WriteLine(sb.ToString(), 1);
                                Notify execution = new Notify();
                                Console.WriteLine("Enviando notificacion por correo ", 1);
                                execution.SendMail(indate, DevicesAvailability, DevicesCapacity, DevicesDetails, ServicesAvailability, ServicesDetails, sb, client, fallidos);
                            }
                        }
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    finally
                    {
                        cmd.Connection.Close();
                        cmd.Connection.Dispose();
                    }
                }
            }
        }
    }
}
