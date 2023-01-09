using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;
using MEDIO.CORE.ConfigElements;
using sophis.utils;

namespace MEDIO.BackOffice.net.src.Utils.Email
{

    public class CSxEmailSender
    {
        #region Members

        private string From { get; set; }
        private string SmtpHost { get; set; }
        private int SmtpPort { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }
        private string Domain { get; set; }
        private bool EnableSsl { get; set; }
        private ERecipientListType RecipientListType { get; set; }

        #endregion

        #region Singleton

        private static CSxEmailSender _emailSender = null;

        private CSxEmailSender(SmtpClientConfigurationElement smtpConfig)
        {
            this.Domain = smtpConfig.Domain;
            this.EnableSsl = smtpConfig.EnableSSL;
            this.From = smtpConfig.From;
            this.Password = smtpConfig.Password;
            this.RecipientListType = smtpConfig.RecipientListType;
            this.SmtpHost = smtpConfig.SMTPHost;
            this.SmtpPort = smtpConfig.SMTPPort;
            this.Username = smtpConfig.Username;
        }

        public static CSxEmailSender GetInstance()
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Begin(typeof(CSxEmailSender).Name, MethodBase.GetCurrentMethod().Name);
                if (_emailSender == null)
                {
                    if (ConfigurationGroup.Current.SMTPClient == null)
                        logger.Write(CSMLog.eMVerbosity.M_error, "SMTPClient is not configuraded properly! Email will not be sent");
                    else _emailSender = new CSxEmailSender(ConfigurationGroup.Current.SMTPClient);
                }
                return _emailSender;
            }
        }

        #endregion

        public void Send(MailMessage message)
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Begin(typeof(CSxEmailSender).Name, MethodBase.GetCurrentMethod().Name);
                logger.Write(CSMLog.eMVerbosity.M_debug, "Begin");

                if (String.IsNullOrEmpty(this.SmtpHost))
                {
                    logger.Write(CSMLog.eMVerbosity.M_warning, "Mail sending is not configured properly.");
                    return;
                }
                //if (String.IsNullOrEmpty(this.From) || String.IsNullOrEmpty(this.SmtpHost) || String.IsNullOrEmpty(this.Username))
                //{
                //    logger.Write(CSMLog.eMVerbosity.M_warning, "Mail sending is not configured properly.");
                //    return;
                //}
                SmtpClient client = new SmtpClient();
                client.Host = this.SmtpHost;
                client.Port = this.SmtpPort;
                client.EnableSsl = this.EnableSsl;
                if (!string.IsNullOrEmpty(this.Username))
                {
                    NetworkCredential credential = new NetworkCredential(this.Username, this.Password, this.Domain);
                    client.Credentials = credential;
                }
                try
                {
                    logger.Write(CSMLog.eMVerbosity.M_debug, "Sending email ... ");
                    // this will solve a remote certificate invalid issue 
                    ServicePointManager.ServerCertificateValidationCallback =
                        delegate(object s, X509Certificate certificate,
                            X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        { return true; };
                    client.Send(message);
                    logger.Write(CSMLog.eMVerbosity.M_debug, "Email was sent successfully");
                }
                catch (Exception e)
                {
                    logger.Write(CSMLog.eMVerbosity.M_error, String.Format("SMTP Server connection is not configured properly : {0}", e.Message));
                    logger.Write(CSMLog.eMVerbosity.M_error, String.Format("Host : {0}, Port : {1}, EnableSsl: {2}, Username : {3}, Domain : {4}", this.SmtpHost, this.SmtpPort, this.EnableSsl, this.Username, this.Domain));
                }
            }
        }

        public string GetFrom()
        {
            return this.From;
        }

    }
}
