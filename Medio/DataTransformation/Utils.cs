using NLog;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
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
                foreach (var filePath in attachments)
                {
                    var attachment = new Attachment(filePath);
                    mailMessage.Attachments.Add(attachment);
                }
            }
            smtpClient.Send(mailMessage);
        }

        public static void RunCommandLineWithOutputFile(string commandLine, string commandLineArgs, string outputFile)
        {
            if (!string.IsNullOrEmpty(commandLine))
            {
                Logger.Debug($"PostTransCommandLine={commandLine}, PostTransCommandLineArgs={commandLineArgs}");
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
                Logger.Debug($"Command Line: {commandLine} {args}");
                try
                {
                    RunCommandLine(commandLine, args);
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
            Logger.Debug($"BEGIN(key={key}, value={value})");
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
            Logger.Debug($"END");
        }

        public static int AuditStart(string transName, string transType, string inputFile, string outputFile, string failureFile = null)
        {
            Logger.Debug($"BEGIN({transName}, {transType})");
            int auditId = 0;
            try
            {
                using (var command = new OracleCommand($@"
BEGIN
    INSERT INTO DTS(ID, TRANS_NAME, TRANS_TYPE, START_TIME, INPUT_FILE, OUTPUT_FILE, FAILURE_FILE)
    VALUES (DTS_SEQ.NEXTVAL, :transName, :transType, SYSDATE, :inputFile, :outputFile, :failureFile)
    RETURNING ID INTO :p_RC;
END;", DataTransformationService.DbConnection))
                {
                    command.Parameters.Add(new OracleParameter("transName", OracleDbType.Varchar2) { Value = transName});
                    command.Parameters.Add(new OracleParameter("transType", OracleDbType.Varchar2) { Value = transType});
                    command.Parameters.Add(new OracleParameter("inputFile", OracleDbType.Varchar2) { Value = inputFile});
                    command.Parameters.Add(new OracleParameter("outputFile", OracleDbType.Varchar2) { Value = outputFile});
                    command.Parameters.Add(new OracleParameter("failureFile", OracleDbType.Varchar2) { Value = failureFile});
                    command.Parameters.Add(new OracleParameter("p_RC", OracleDbType.Int32, System.Data.ParameterDirection.Output));
                    command.ExecuteNonQuery();
                    auditId = ((OracleDecimal)command.Parameters["p_RC"].Value).ToInt32();
                }
            } catch(Exception e)
            {
                Logger.Error(e, "Error when inserting in Audit DTS");
            }
            Logger.Debug($"END({auditId})");
            return auditId;
        }

        public static void AuditEnd(int auditId, int totalLines, int processedLines, int failureLines = 0)
        {
            Logger.Debug($"BEGIN(auditId={auditId})");
            try
            {
                using (var command = new OracleCommand(@"
UPDATE DTS SET END_TIME=SYSDATE,
    DURATION = TO_CHAR(TRUNC((SYSDATE - START_TIME) * 24), 'FM00') || ':' || 
       TO_CHAR(ROUND(MOD((SYSDATE - START_TIME) * 1440, 60)), 'FM00') || ':' || 
       TO_CHAR(ROUND(MOD((SYSDATE - START_TIME) * 86400, 60)), 'FM00'),
    TOTAL_LINES=:totalLines, PROCESSED_LINES=:processedLines, FAILURE_LINES=:failureLines WHERE ID=:auditId", DataTransformationService.DbConnection))
                {
                    command.Parameters.Add(new OracleParameter("totalLines", OracleDbType.Int32) { Value = totalLines });
                    command.Parameters.Add(new OracleParameter("processedLines", OracleDbType.Int32) { Value = processedLines });
                    command.Parameters.Add(new OracleParameter("failureLines", OracleDbType.Int32) { Value = failureLines });
                    command.Parameters.Add(new OracleParameter("auditId", OracleDbType.Int32) { Value = auditId });
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error when inserting in Audit DTS");
            }
            Logger.Debug($"END");
        }
    }
}
