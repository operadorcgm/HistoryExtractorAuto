using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace HistoryExtractorAuto.Util
{
    public class Logmakerr
    {
        private StreamWriter log;
        private FileStream fileStream = null;
        private DirectoryInfo logDirInfo = null;
        private FileInfo logFileInfo;
        private string _Cliente_;
        

        public Logmakerr(String Servicio)
        {
            _Cliente_ = Servicio;
        }

        public void WriteLog(string strLog, int ConsoleFlag)
        {
            if (ConsoleFlag == 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;

            }
            else if (ConsoleFlag == 2)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine(strLog);
            Console.ForegroundColor = ConsoleColor.White;              
            //string logFilePath = "C:\\logsPRTG\\";

            DateTime dt = DateTime.Now;
            CultureInfo iv = CultureInfo.InvariantCulture;
            //Console.WriteLine(dt.ToString("G", iv));
            string logFilePath = "C:\\logsPRTG\\";
            logFilePath = logFilePath + dt.ToString("yyyyMMMMdd", iv) + "_" + _Cliente_ + "_." + "txt";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.WriteLine(DateTime.Now.ToString("HH:mm:ss tt") + strLog);
            log.Close();
            
        }

        public void DisposeConn()
        {
            log.Dispose();
        }
    }
}
