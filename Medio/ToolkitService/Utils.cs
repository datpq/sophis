using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace ToolkitService
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
                Logger.Info($"Reporting error to {errEmailRcptTo} ({failureFile})");
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
                foreach (var filePath in attachments)
                {
                    var attachment = new Attachment(filePath);
                    mailMessage.Attachments.Add(attachment);
                }
            }
            smtpClient.Send(mailMessage);
        }

        public static void RunCommandLineWithOutputFile(string commandLine, string commandLineArgs, string outputFile, string targetFile = null)
        {
            if (!string.IsNullOrEmpty(commandLine))
            {
                Logger.Info($"PostTransCommandLine={commandLine}, PostTransCommandLineArgs={commandLineArgs}, outputFile={outputFile}, targetFile={targetFile}");
                var args = $"\"{outputFile}\"";
                if (!string.IsNullOrEmpty(commandLineArgs))
                {
                    if (commandLineArgs.Contains("@OutputFile"))
                    {
                        args = commandLineArgs.Replace("@OutputFile", args);
                    }
                    else
                    {
                        args = commandLineArgs + " " + args;
                    }
                }
                args = args.Replace("@TargetFile", targetFile);
                Logger.Info($"Command Line: {commandLine} {args}");
                try
                {
                    RunCommandLine(commandLine, args);
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error when executing the command line");
                }
            }
        }

        public static void RunCommandLine(string fileName, string args)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = args;
            p.Start();
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

        public static string GetCsvVal(string[] csvHeaders, string[] csvVals, string columnName)
        {
            int idx = Enumerable.Range(0, csvHeaders.Length)
                    .Where(i => columnName.Equals(csvHeaders[i])).FirstOrDefault();

            return csvVals[idx];
        }

        public static string PutTimestamp(this string path)
        {
            string ext = Path.GetExtension(path);
            string ans = $"{path.Substring(0, path.Length - ext.Length)}_{DateTime.Now.ToString("yyyyMMddhhmmss")}{ext}";
            return ans;
        }

        public static void CopyToDestFile(string filePath, string destFile)
        {
            Logger.Info($"Copying file: {filePath} to {destFile}");
            try
            {
                if (!Path.IsPathRooted(destFile))
                {
                    destFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), destFile);
                }
                File.Copy(filePath, destFile, true);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error while copying file");
            }
        }

        public static void MoveToDestFile(string filePath, string destFile)
        {
            Logger.Info($"Moving file: {filePath} to {destFile}");
            try
            {
                if (!Path.IsPathRooted(destFile))
                {
                    destFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), destFile);
                }
                var processedFile = destFile;
                if (!File.Exists(processedFile))
                {
                    Logger.Info($"Renaming file to {processedFile}...");
                    File.Move(filePath, processedFile);
                }
                else
                {
                    int count = 1;
                    while (count <= 1000)
                    {
                        processedFile = $"{destFile}.{count++}";
                        if (!File.Exists(processedFile))
                        {
                            Logger.Info($"Renaming file to {processedFile}...");
                            File.Move(filePath, processedFile);
                            break;
                        }
                    }
                    if (count > 1000)
                    {
                        Logger.Info($"No more space is free. We are forced to delete the file: {filePath}");
                        File.Delete(filePath);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error while renaming file to processed");
            }
        }
    }
}
