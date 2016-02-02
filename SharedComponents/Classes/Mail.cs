using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Mail;

namespace FinalBuild
{
    public class Mail
    {
        public static MailMessage AssembleMail(string from, string[] toList, string subject, string messageText, string[] ccList, string attachmentFilePath)
        {
            return AssembleMail(from, toList, subject, messageText, ccList, attachmentFilePath, null);
        }

        public static MailMessage AssembleMail(string from, string[] toList, string subject, string messageText, string[] ccList, string attachmentFilePath, string fromDisplayName)
        {
            MailMessage mailMessage = new MailMessage();
            string recipientDelimiter = ",";

            if (fromDisplayName != null)
            {
                mailMessage.From = new MailAddress(from, fromDisplayName);
            }
            else
            {
                mailMessage.From = new MailAddress(from);
            }
            mailMessage.To.Add(ConvertArrayToDelimitedString(toList, recipientDelimiter));
            mailMessage.Subject = subject;
            mailMessage.Body = messageText;

            if (ccList != null)
            {
                mailMessage.CC.Add(ConvertArrayToDelimitedString(toList, recipientDelimiter));
            }
            
            if (attachmentFilePath != null && attachmentFilePath != string.Empty && 
                System.IO.File.Exists(attachmentFilePath))
            {
                // Create Attachment
                mailMessage.Attachments.Add(new Attachment(attachmentFilePath));
            }

            return mailMessage;
        }

        /// <summary>
        /// Assumes Network SMTPServer has been specified
        /// </summary>
        /// <param name="message"></param>
        /// <param name="smtpServer"></param>
        public static void SendMail(MailMessage message, string smtpServer)
        {
            SmtpClient smtpClient = new SmtpClient(smtpServer);
            try
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Send(message);
                if (message.Attachments != null)
                {
                    for (int index = 0; index < message.Attachments.Count; index++)
                    {
                        message.Attachments[index].Dispose(); ;
                    }
                }
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        /// <summary>
        /// Assumes local IIS hosted SMTPServer is present
        /// </summary>
        /// <param name="message"></param>
        public static void SendMail(MailMessage message)
        {
            SmtpClient smtpClient = new SmtpClient(System.Net.Dns.GetHostName());

            try
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
                smtpClient.Send(message);
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        private static string ConvertArrayToDelimitedString(string[] targetArray, string delimiterCharacter)
        {
            string commaSeparatedString = string.Empty;

            if (delimiterCharacter == null || delimiterCharacter == string.Empty)
            {
                delimiterCharacter = ",";
            }

            if (targetArray != null)
            {
                for (int intIndex = 0; intIndex < targetArray.Length; intIndex++)
                {
                    commaSeparatedString += targetArray[intIndex] + delimiterCharacter;
                }
                commaSeparatedString = commaSeparatedString.Substring(0,commaSeparatedString.Length - 1);
            }

            return commaSeparatedString;
        }
    }
}
