using HistoryExtractorAuto.Util;
using System;
using System.Globalization;
using System.Text;

using System.Xml.Linq;
using Microsoft.Exchange.WebServices.Autodiscover;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;

namespace HistoryExtractorAuto
{
    class Program
    {
        //Horas de ejecuccion todos los clientes
        private static string inhour00 = "-00-00-00";
        private static string endhour00 = "-23-59-59";
        private static string inhour05 = "-05-00-00";
        private static string endhour05 = "-04-59-59";
        //Horas de ejecuccion CCMA
        private static string inhourhl00 = "-07-00-00";
        private static string endhourhl00 = "-18-59-59";
        //HOras laborales CCMA - desde las 7:00 hasta las 19:00
        private static string inhourhl05 = "-12-00-00";
        private static string endhourhl05 = "-23-59-59";        
        static void Main(string[] args)
        {
            
            Program p = new Program();
            XMLInspector inspector = new XMLInspector();
            
            Console.WriteLine("total Parametros recibidos: " + args.Length);
            //Argumentos /d -x(dias atras) y(hata que dia)
            
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine("Parametro(s) recibido(s): " + args[i]);

            }
            string Pattern = @"(/d)[0-9]([0-9])?";
            string Contructor = "";
            string[] ClientInfo = inspector.getDB_and_prtgURL();
            if (args.Length == 0)
            {
                //p.runBeforeDays(ClientInfo, 25, 1);
                p.runDaily(ClientInfo);
            }
            else
            {
                foreach (string El in args)
                {
                    Contructor += El;
                }
                //Console.WriteLine("Contructor: " + Contructor);        
                MatchCollection matches = Regex.Matches(Contructor, Pattern, RegexOptions.IgnorePatternWhitespace);
                Console.WriteLine("Found {0} matches.", matches.Count);
                if (args[0].Equals("/Help") || args[0].Equals("/help") || args[0].Equals("-Help") || args[0].Equals("-help") || args[0].Equals("Help") || args[0].Equals("help"))
                {
                    Console.WriteLine("Uso: HistoryExtractorAuto [Inicializacion] Requiere [valor] [valor]");
                    Console.WriteLine("\tOpcion:");
                    Console.WriteLine("\t\t-/d requerido, traduce dias [Inicializacion]");
                    Console.WriteLine("\t\t-[Numero entero] requerido, desde - dias a extraer (fecha Inicio) [valor]");
                    Console.WriteLine("\t\t-[Numero entero] opcional, hasta - dias a extraer (fecha fin) [valor]");
                    Console.WriteLine("\t-Ejemplo:");
                    Console.WriteLine("\t\t-/d 25 1:");
                    Console.WriteLine("\t\tEjemplo de fecha de ejecucion 26/01/2020, el primer nuemro resta 25 dias a la fecha de ejecucion\n" +
                        "La fecha de inicio de la extracion seria desde el 01/01/2020\n" +
                        "y finaliza el 26/01/2020");
                }
                else if (matches.Count > 0 && args.Length == 2)
                {
                    
                    p.runBeforeDays(ClientInfo, Int32.Parse(args[1]), 1);
                }    
                else if(matches.Count > 0 && args.Length == 3)
                {
                    p.runBeforeDays(ClientInfo, Int32.Parse(args[1]), Int32.Parse(args[2]));
                }
                else
                {
                    Console.WriteLine("Argumentos no validos, use Help para mas informacion.");
                }
            }    
        }

        public void runDaily(string[] ClientInfo)
        {
            Console.WriteLine("Start runDaily");
            int fallidos = 0;
            StringBuilder sb = new StringBuilder(); //contructor de String para el log
            
            DateTime yesterday = DateTime.Now.AddDays(-1);
            string indate = yesterday.ToString("yyyy-MM-dd");
            //Logmakerr Lm = new Logmakerr(ClientInfo[0]); //instacia logmakerr
            CultureInfo provider = CultureInfo.InvariantCulture;
            DateTime today = DateTime.ParseExact(indate, "yyyy-MM-dd", provider).AddDays(1);
            string enddate = today.ToString("yyyy-MM-dd");
            //#region Nombre de Cliente en log y pantalla                        
            if (!ClientInfo[0].StartsWith("Error"))
            {
                //string Cliente = dbname.Substring(0); //SubString catch name like xxxxx_prod
                //Delimiter = '_';
                //SeparateString = Cliente.Split(Delimiter);
                //#endregion                
                Data execution = new Data();
                Console.WriteLine("***************Nuevo Daily***************", 1);
                Console.WriteLine("************Cliente: " + ClientInfo[0] + "***************", 1);
                Console.WriteLine("Execution ExtractDeviceAvailabilit", 2);
                execution.ExtractAvailability(indate, enddate, "SP_ExtractDeviceAvailability", "SP_SaveDeviceAvailability", "SP_SaveDeviceAvailabilityDetails", sb, ClientInfo[1], inhour00, endhour00, inhour05, endhour05);
                Console.WriteLine("Execution ExtractServiceAvailability", 2);
                execution.ExtractAvailability(indate, enddate, "SP_ExtractServiceAvailability", "SP_SaveServiceAvailability", "SP_SaveServiceAvailabilityDetails", sb, ClientInfo[1], inhour00, endhour00, inhour05, endhour05);
                Console.WriteLine("Execution ExtractDeviceCapacity", 2);
                execution.ExtractCapacity(indate, "SP_ExtractDeviceCapacity", "SP_SaveDeviceCapacity", sb,  fallidos, ClientInfo[1], inhour00, endhour00);
                if(ClientInfo[0].Equals("ccma"))
                {
                    Console.WriteLine("************CCMA -12H***********", 1);
                    execution.ExtractAvailability(indate, enddate, "SP_ExtractDeviceAvailability", "SP_SaveDeviceAvailabilityHL", "SP_SaveDeviceAvailabilityDetailsHL", sb, ClientInfo[1], inhourhl00, endhourhl00, inhourhl05, endhourhl05);
                    execution.ExtractAvailability(indate, enddate, "SP_ExtractServiceAvailability", "SP_SaveServiceAvailabilityHL", "SP_SaveServiceAvailabilityDetailsHL", sb, ClientInfo[1], inhourhl00, endhourhl00, inhourhl05, endhourhl05);
                    execution.ExtractCapacity(indate, "SP_ExtractDeviceCapacity", "SP_SaveDeviceCapacityHL", sb,  fallidos, ClientInfo[1], inhourhl00, endhourhl00);
                }
                fallidos = execution._fallidos;
                execution.CountData(indate, sb, ClientInfo[0], fallidos);
            }
            else
            {
                Console.WriteLine("**************"+ClientInfo[0]+"***************", 1);
            }
            Console.WriteLine("Saliendo del programa", 1);
            
            Environment.Exit(1);
        }

        public void runBeforeDays(string[] ClientInfo, int dayOld, int dayEnd)
        {
            //Descomentar este bloque para traer dados de varios dias atras consecutivamente

            //int dayOld = -3;
            //Horas de ejecucion TODOS LOS CLIENTES

            Console.WriteLine("Start runBeforeDays");
            int fallidos = 0;
            dayOld = Math.Abs(dayOld) * (-1);
            if (dayEnd != 1)
            {
                dayEnd = Math.Abs(dayEnd) * (-1);
            }
            //Console.WriteLine(dayOld + " "+ dayEnd);
            DateTime yesterday;
            DateTime today;
            string indate;
            string enddate;
            Data execution = new Data();
            CultureInfo provider;
            StringBuilder sb = new StringBuilder(); //contructor de String para el log
            //Logmakerr Lm = new Logmakerr(ClientInfo[0]); //instacia logmakerr            
            Console.WriteLine("***************Nuevo BeforeDays***************", 1);
            Console.WriteLine("************Cliente: " + ClientInfo[0] + "***************", 1);            
            if (!ClientInfo[0].StartsWith("Error"))
            {                
                //Logmakerr Lm = new Logmakerr(ClientInfo[0]); //instacia logmakerr
                for (int i = dayOld; i <= dayEnd; i++)
                {                    
                    yesterday = DateTime.Now.AddDays(i);
                    indate = yesterday.ToString("yyyy-MM-dd");
                    provider = CultureInfo.InvariantCulture;
                    today = DateTime.ParseExact(indate, "yyyy-MM-dd", provider).AddDays(1);
                    enddate = today.ToString("yyyy-MM-dd");                    
                    Console.WriteLine("************Dia(s): " + i + "***************", 1);
                    execution.ExtractAvailability(indate, enddate, "SP_ExtractDeviceAvailability", "SP_SaveDeviceAvailability", "SP_SaveDeviceAvailabilityDetails", sb, ClientInfo[1], inhour00, endhour00, inhour05, endhour05);
                    execution.ExtractAvailability(indate, enddate, "SP_ExtractServiceAvailability", "SP_SaveServiceAvailability", "SP_SaveServiceAvailabilityDetails", sb, ClientInfo[1], inhour00, endhour00, inhour05, endhour05);
                    execution.ExtractCapacity(indate, "SP_ExtractDeviceCapacity", "SP_SaveDeviceCapacity", sb, fallidos, ClientInfo[1], inhour00, endhour00);
                    //execution.CountData(indate);
                }
                if(ClientInfo[0].Equals("ccma"))
                {
                    Console.WriteLine("************CCMA -12H***********", 1);
                    for (int i = dayOld; i <= dayEnd; i++)
                    {
                        yesterday = DateTime.Now.AddDays(i);
                        indate = yesterday.ToString("yyyy-MM-dd");

                        provider = CultureInfo.InvariantCulture;
                        today = DateTime.ParseExact(indate, "yyyy-MM-dd", provider).AddDays(1);
                        enddate = today.ToString("yyyy-MM-dd");
                        
                        Console.WriteLine("************Dia(s): " + i + "***************", 1);
                        execution.ExtractAvailability(indate, enddate, "SP_ExtractDeviceAvailability", "SP_SaveDeviceAvailabilityHL", "SP_SaveDeviceAvailabilityDetailsHL", sb, ClientInfo[1], inhourhl00, endhourhl00, inhourhl05, endhourhl05);
                        execution.ExtractAvailability(indate, enddate, "SP_ExtractServiceAvailability", "SP_SaveServiceAvailabilityHL", "SP_SaveServiceAvailabilityDetailsHL", sb, ClientInfo[1], inhourhl00, endhourhl00, inhourhl05, endhourhl05);
                        execution.ExtractCapacity(indate, "SP_ExtractDeviceCapacity", "SP_SaveDeviceCapacityHL", sb, fallidos, ClientInfo[1], inhourhl00, endhourhl00);
                        //execution.ExtractAvailability(indate, enddate, "SP_ExtractDeviceAvailability", "SP_SaveDeviceAvailability", "SP_SaveDeviceAvailabilityDetails", sb,  ClientInfo[1], inhourhl00, endhourhl00, inhourhl05, endhourhl05);
                        //execution.ExtractAvailability(indate, enddate, "SP_ExtractServiceAvailability", "SP_SaveServiceAvailability", "SP_SaveServiceAvailabilityDetails", sb,  ClientInfo[1], inhourhl00, endhourhl00, inhourhl05, endhourhl05);
                        //execution.ExtractCapacity(indate, "SP_ExtractDeviceCapacity", "SP_SaveDeviceCapacity", sb,  fallidos, ClientInfo[1], inhourhl00, endhourhl00);
                    }
                }

            }
            else
            {
                Console.WriteLine("**************" + ClientInfo[0] + "***************", 1);
            }
            Console.WriteLine("Saliendo del programa", 1);
            
            Environment.Exit(1);
        }

        private void ExecutionExtract(string indate, string enddate, StringBuilder sb, int fallidos, string ClientInfo, string inhour00, string endhour00, string inhour05, string endhour05)
        {
            //execution.ExtractAvailability(indate, enddate, "SP_ExtractDeviceAvailability", "SP_SaveDeviceAvailability", "SP_SaveDeviceAvailabilityDetails", sb,  fallidos, ClientInfo[1], inhour00, endhour00, inhour05, endhour05);
            //execution.ExtractAvailability(indate, enddate, "SP_ExtractServiceAvailability", "SP_SaveServiceAvailability", "SP_SaveServiceAvailabilityDetails", sb,  fallidos, ClientInfo[1], inhour00, endhour00, inhour05, endhour05);
            //execution.ExtractCapacity(indate, "SP_ExtractDeviceCapacity", "SP_SaveDeviceCapacity", sb,  fallidos, ClientInfo[1], inhour00, endhour00);
        }
    }
}