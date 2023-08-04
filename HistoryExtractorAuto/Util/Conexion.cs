using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Xml;


namespace HistoryExtractorAuto.Util
{
    class Conexion
    {
        public SqlConnection connection = new SqlConnection();
        private static string[] clientInfo;        
        private static string ScrtCon;

        public Conexion()
        {
            clientInfo = new string[2];
            string dbtSelectorAppSeting = "0";
            XmlDocument xmlID = new XmlDocument();
            xmlID.Load("IDSelector.xml");
            XmlNodeList nodeList = xmlID.DocumentElement.SelectNodes("/DataExtractor/client/IDExtractor");//primero el ID
            foreach (XmlNode node in nodeList)
            {
                dbtSelectorAppSeting = node.SelectSingleNode("ID").InnerText;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("clientSelector.xml");
            //XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/catalog/client/connectinfo");
            nodeList = xmlDoc.DocumentElement.SelectNodes("/catalog/client/connectinfo");//Segundo, el PRTG y Base de datos
            string XMLId = "";
            bool finIt = false;
            foreach (XmlNode node in nodeList)
            {
                XMLId = node.SelectSingleNode("idclient").InnerText;
                if (XMLId.Equals(dbtSelectorAppSeting))
                {
                    clientInfo[0] = node.SelectSingleNode("dbProd").InnerText;
                    //clientInfo[1] = node.SelectSingleNode("prtgURL").InnerText;
                    finIt = true;
                }

            }
            if (finIt)
            {
                ScrtCon = ConfigurationManager.ConnectionStrings["HistoryExtractorConnectionString"].ToString();
                ScrtCon = string.Format(ScrtCon, clientInfo[0]);
            }
        }
        public SqlConnection Connect()
        {
            
            //string[] clientInfo = new string[2];
            //string connectionString = ConfigurationManager.
            //    ConnectionStrings["HistoryExtractorConnectionString"].
            //    ConnectionString;
            //char Delimiter = ';';
            //string[] SeparateString;
            //string dbtSelectorAppSeting = ConfigurationManager.AppSettings.GetKey(0);
            //string dbtSelectorAppSeting = "0";
            //XmlDocument xmlID = new XmlDocument();
            //xmlID.Load("IDSelector.xml");
            //XmlNodeList nodeList = xmlID.DocumentElement.SelectNodes("/DataExtractor/client/IDExtractor");//primero el ID
            //foreach (XmlNode node in nodeList)
            //{
            //    dbtSelectorAppSeting = node.SelectSingleNode("ID").InnerText;
            //}

            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load("clientSelector.xml");
            ////XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/catalog/client/connectinfo");
            //nodeList = xmlDoc.DocumentElement.SelectNodes("/catalog/client/connectinfo");//Segundo, el PRTG y Base de datos
            //string XMLId = "";
            //bool finIt = false;
            //foreach (XmlNode node in nodeList)
            //{
            //    XMLId = node.SelectSingleNode("idclient").InnerText;
            //    if (XMLId.Equals(dbtSelectorAppSeting))
            //    {
            //        clientInfo[0] = node.SelectSingleNode("dbProd").InnerText;
            //        clientInfo[1] = node.SelectSingleNode("prtgURL").InnerText;
            //        finIt = true;
            //    }

            //}
            //if (finIt)
            //{
            //    string ScrtCon = ConfigurationManager.ConnectionStrings["HistoryExtractorConnectionString"].ToString();
            //    ScrtCon = string.Format(ScrtCon, clientInfo[0]);
            connection = new SqlConnection(ScrtCon);
                //string Cliente = clientInfo[0].Substring(0); //SubString catch name like xxxxx_prod
                //Delimiter = '_';
                //SeparateString = Cliente.Split(Delimiter);
                //clientInfo[0] = SeparateString[0];              
         
            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                throw ex;
            }





            //    string dbtSelectorAppSeting = ConfigurationManager.AppSettings.GetKey(0);
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load("clientSelector.xml");
            //XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/catalog/client/connectinfo");
            //string XMLId = "", dbname = "";         
            //foreach (XmlNode node in nodeList)
            //{
            //    XMLId = node.SelectSingleNode("idclient").InnerText;
            //    if (XMLId.Equals(dbtSelectorAppSeting))
            //    {
            //        dbname = node.SelectSingleNode("dbProd").InnerText;
            //        string ScrtCon = ConfigurationManager.ConnectionStrings["HistoryExtractorConnectionString"].ToString();
            //        ScrtCon = string.Format(ScrtCon, dbname);
            //        connection = new SqlConnection(ScrtCon);
            //    }

            //}
            //string ConString = ConfigurationManager.ConnectionStrings["HistoryExtractorConnectionString"].ConnectionString; //Save Connnection String to local Variable 

        }
    }
}
