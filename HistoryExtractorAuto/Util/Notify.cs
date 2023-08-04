using System;
using System.Collections.Generic;
using System.Text;
using MailKit.Security;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;


namespace HistoryExtractorAuto.Util
{
    class Notify
    {
        /// <summary>
        /// Metodo para realizar la consulta de los datos agregados al final del proceso y envia el resultado por e-mail.
        /// </summary>
        /// <param name="indate"></param>
        /// <param name="DevAvailability"></param>
        /// <param name="DevCapacity"></param>
        /// <param name="DevDetails"></param>
        /// <param name="ServAvailability"></param>
        /// <param name="ServDetails"></param>
        public void SendMail(string indate, int DevAvailability, int DevCapacity, int DevDetails, int ServAvailability, int ServDetails, StringBuilder sb, string client, int fallidos)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("operadorcgm@gmail.com"));
                email.To.Add(MailboxAddress.Parse("cgm2@e-global.com.co"));
                email.Subject = "History Extractor - " + client;
                email.Body = new TextPart(TextFormat.Plain)
                {
                    Text = "La ejecucion terminó, se insertaron los siguientes registros:" +
                        "\r\n" + "\r\n" + "DeviceAvailability: " + DevAvailability +
                        "\r\n" + "DevicesCapacity: " + DevCapacity +
                        "\r\n" + "DevicesDetails: " + DevDetails +
                        "\r\n" + "ServicesAvailability: " + ServAvailability +
                        "\r\n" + "ServicesDetails: " + ServDetails +
                        "\r\n" + "\r\n" + "Fecha: " + indate +
                        "\r\n" + "Fallidos " + fallidos +
                        "\r\n" + sb.ToString()
            };

                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 465, true);
                smtp.Authenticate("operadorcgm@gmail.com", "lpaslyffimpvgwim");
                smtp.Send(email);
                smtp.Disconnect(true);
                Console.WriteLine("Message Sent Succesfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        }

        public void SendMail(int idSensor, string error, Exception exception)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("operadorcgm@gmail.com"));
                email.To.Add(MailboxAddress.Parse("cgm2@e-global.com.co"));
                email.Subject = "History Extractor - E-Global - Error!!!";
                email.Body = new TextPart(TextFormat.Plain)
                {
                    Text = "La ejecucion terminó, Error en la API:" +
                    "\r\n" + "\r\n" + "ID de sensor: " + idSensor +
                    "\r\n" + "\r\n" + "Error API: " + error +
                    "\r\n" + "\r\n" + "Mensaje de error: " + exception.Message +
                    "\r\n" + "\r\n" + "Linea donde falló: " + exception.StackTrace
                };

                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 465, SecureSocketOptions.StartTls);
                smtp.Authenticate("operadorcgm@gmail.com", "lpaslyffimpvgwim");
                smtp.Send(email);
                smtp.Disconnect(true);
                Console.WriteLine("Message Sent Succesfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
