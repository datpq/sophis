using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataTransformation
{
    public static class Utils
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void SendErrorEmail(string errEmailubject, string failureFile)
        {
            string errEmailRcptTo = ConfigurationManager.AppSettings["ErrEmailRecipientTo"];
            string errEmailRcptCC = ConfigurationManager.AppSettings["ErrEmailRecipientCC"];
            string errEmailBody = ConfigurationManager.AppSettings["ErrEmailBody"];
            if (!string.IsNullOrEmpty(errEmailRcptTo) && !string.IsNullOrEmpty(errEmailubject) && !string.IsNullOrEmpty(errEmailBody))
            {
                Logger.Debug($"Reporting error to {errEmailRcptTo} ({failureFile})");
                try
                {
                    SendEmail(errEmailRcptTo, errEmailubject, errEmailBody, errEmailRcptCC, new[] { failureFile });
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error when sending email");
                }
            }
        }

        public static void SendEmail(string recipientTo, string subject, string body, string recipientCC = null, string[] attachments = null)
        {
            var smtpHost = ConfigurationManager.AppSettings["SMTPHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTPPort"]);
            var from = ConfigurationManager.AppSettings["SMTPFrom"];
            var username = ConfigurationManager.AppSettings["SMTPUsername"];
            var password = ConfigurationManager.AppSettings["SMTPPassowrd"];
            var domain = ConfigurationManager.AppSettings["SMTPDomain"];
            var enableSSL = bool.Parse(ConfigurationManager.AppSettings["SMTPEnableSSL"]);
            var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(username, password, domain),
                EnableSsl = enableSSL,
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(recipientTo);
            if (!string.IsNullOrEmpty(recipientCC)) mailMessage.CC.Add(recipientCC);
            if (attachments != null)
            {
                foreach(var filePath in attachments)
                {
                    var attachment = new Attachment(filePath);
                    mailMessage.Attachments.Add(attachment);
                }
            }
            smtpClient.Send(mailMessage);
        }

        public static bool IsFileClosed(string filepath, bool wait = true)
        {
            bool fileClosed = false;
            int retries = 20;
            const int delay = 500; // Max time spent here = retries*delay milliseconds

            if (!File.Exists(filepath))
                return false;

            do
            {
                try
                {
                    // Attempts to open then close the file in RW mode, denying other users to place any locks.
                    FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    fs.Close();
                    fileClosed = true; // success
                }
                catch (IOException) { }

                if (!wait) break;

                retries--;

                if (!fileClosed)
                {
                    Logger.Debug(string.Format("Retry opening file: {0}", filepath));
                    Thread.Sleep(delay);
                }
            }
            while (!fileClosed && retries > 0);
            return fileClosed;
        }

        public static IEnumerable<string> ReadLines(Func<Stream> streamProvider, Encoding encoding)
        {
            using (var stream = streamProvider())
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static string ReadAllTextFile(string inputFile)
        {
            string result = string.Empty;
            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, inputFile)))
            {
                if (!IsFileClosed(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, inputFile)))
                {
                    Logger.Error($"Can not open file: {inputFile}");
                    return result;
                }
                result = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, inputFile));
            }
            else
            {
                var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + inputFile));
                if (resourceName == null) throw new ArgumentException($"Template file not found in resource: {inputFile}");
                var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }

        public static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Logger.Error("Error writing app settings");
            }
        }
    }
}
