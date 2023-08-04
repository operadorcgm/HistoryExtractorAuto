using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Configuration;

namespace HistoryExtractorAuto.Util
{
    public class XMLInspector
    {
        public string[] getDB_and_prtgURL()
        {
            string[] clientInfo = new string[2];
            try
            {
                char Delimiter = ';';              
                string[] SeparateString;
                //string dbtSelectorAppSeting = ConfigurationManager.AppSettings.GetKey(0);
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
                        clientInfo[1] = node.SelectSingleNode("prtgURL").InnerText;
                        finIt = true;
                        break;
                    }

                }
                if (finIt)
                {
                    string Cliente = clientInfo[0].Substring(0); //SubString catch name like xxxxx_prod
                    Delimiter = '_';
                    SeparateString = Cliente.Split(Delimiter);
                    clientInfo[0] = SeparateString[0];
                    return clientInfo;
                }
            }
            catch (Exception ex)
            {
                clientInfo[0] = "Error e ejecuccion: get DB and prtg URL:";
                clientInfo[1] = ex.Message ;
                return clientInfo;             
            }
            clientInfo[0] = "Error e ejecuccion: get DB and prtg URL - Indeterminado";
            clientInfo[1] = "";
            return clientInfo;
        }
    }
}
