using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection;
using MEDIO.BackOffice.net.src.Utils.Email;
using MEDIO.CORE.ConfigElements;
using sophis.backoffice_kernel;
using sophis.portfolio;
using sophis.tools;
using sophis.utils;
using MailMessage = System.Net.Mail.MailMessage;


namespace MEDIO.BackOffice.net.src.KernelEngine
{
    public class CSxEmailKernelEngine : sophis.backoffice_kernel.CSMKernelEngine
    {
        private const string medioOfficeAddress = "The Exchange, 4th Floor, George’s Dock, I.F.S.C, Dublin 1, DO1 P2V6 Ireland";

        public override void Run(CSMTransaction original, CSMTransaction final, ArrayList recipientType, eMGenerationType generationType, CSMEventVector mess, int event_id)
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Begin(typeof(CSxEmailKernelEngine).Name, MethodBase.GetCurrentMethod().Name);
                logger.Write(CSMLog.eMVerbosity.M_debug, "Begin");
                try
                {
                    var report = new CSxMedioEmailReport(final);
                    var emailSender = CSxEmailSender.GetInstance();
                    var msg = ConstructMailMessage(emailSender, report, final);
                    if (IsMirroredTrade(final))
                    {
                        logger.Write(CSMLog.eMVerbosity.M_debug, "No email will be sent as the trade is mirrored!");
                    }
                    else
                    {
                        logger.Write(CSMLog.eMVerbosity.M_debug, "Trade " + final.getInternalCode() + " is not a mirrored trade. Sending email ...");
                        emailSender.Send(msg);
                    }
                }
                catch (Exception e)
                {
                    logger.Write(CSMLog.eMVerbosity.M_error, "Exception : " + e.Message);
                    logger.Write(CSMLog.eMVerbosity.M_error, "Exception : " + e.StackTrace);
                }
                logger.Write(CSMLog.eMVerbosity.M_debug, "End");
            }
        }

        #region private
        private MailMessage ConstructMailMessage(CSxEmailSender sender, CSxMedioEmailReport report, CSMTransaction trade)
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Begin(typeof(CSxEmailKernelEngine).Name, MethodBase.GetCurrentMethod().Name);

                var cashTypeStr = Enum.GetName(typeof(ECashType), report.Type);
                string subject = String.Format("{0} {1} - {2} Trade ID", report.FundName, cashTypeStr, trade.getInternalCode());
                string from = sender.GetFrom();

                var mailMessage = new MailMessage
                {
                    Subject = subject,
                    IsBodyHtml = true,
                    Body = CreateEmailBody(report)
                };

                foreach (var oneEmail in report.GetDelegateMangersEmailFromAccount())
                    mailMessage.To.Add(oneEmail);
                
                MailAddress fromAddress = new MailAddress(from);
                mailMessage.From = fromAddress;
                
                logger.Write(CSMLog.eMVerbosity.M_debug, "End");
                logger.End();
                return mailMessage;
            }
        }

        /// <summary>
        /// A proper way would be to use html and xlst templates, method below is rather a quick-win (or a questionable implementation)
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="thirdParty"></param>
        /// <returns></returns>
        private string CreateEmailBody(CSxMedioEmailReport report)
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Begin(typeof(CSxEmailKernelEngine).Name, MethodBase.GetCurrentMethod().Name);
                logger.Write(CSMLog.eMVerbosity.M_debug, "Begin");
                string dateFormat = "dd MMMM, yyyy";
                string body = "";

                // Manager name 
                body += String.Format("To:  {0} <br /><br />", report.MangerName);
                body += "Please find below the <b>PRE ADVICE</b> for your reply and Subscription/Redemptions on the specific funds with the following economics: <br /><br /><br />";

                // Fund info
                var cashTypeStr = Enum.GetName(typeof(ECashType), report.Type);
                body += String.Format("<b>{0}</b> - Cash <b><i>{1}</i></b> <br /><br /><br />", report.FundName, cashTypeStr);

                // Trade amount 
                body += String.Format("{0} Amount {1} :  {2} <br /><br />", cashTypeStr, report.Currency, String.Format("{0:n}", report.Amount));

                // Trade date 
                body += String.Format("Trade date :  {0} {1}<br /><br />", report.TradeDate.DayOfWeek, report.TradeDate.ToString(dateFormat));

                // Settlement date 
                body += String.Format("Settlement date :  {0} {1}<br /><br />", report.SettlementDate.DayOfWeek, report.SettlementDate.ToString(dateFormat));

                // Reference 
                body += "Reference :  Manager Transfer <br /><br />";

                // Debit RBC Custody AC 
                //body += String.Format("Debit RBC Custody A/C :  {0} - {1} <br /><br />", report.DebitCustodyAccount, report.DebitCustodyName);

                // Credit RBC Custody AC 
                //body += String.Format("Credit RBC Custody A/C :  {0} - {1} <br /><br /><br />", report.CreditCustodyAccount, report.CreditCustodyName);

                body += "Please advise/approve by reply the economic details above and a dual signed email will be sent after approval is received. <br /><br />";

                // Signature
                body += @"Regards <br /><br /><img src='http://www.mifl.ie/assets/images/logo-mifl-1x.png'><br />";
                body += String.Format("{0} | {1} <br />", ConfigurationGroup.Current.SMTPClient.MAMLEmailAddress, ConfigurationGroup.Current.SMTPClient.MAMLWebAddress);
                body += medioOfficeAddress;

                logger.Write(CSMLog.eMVerbosity.M_debug, "End");
                logger.End();
                return body;
            }
        }

        private bool IsMirroredTrade(sophis.portfolio.CSMTransaction tr)
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Begin(typeof(CSxEmailKernelEngine).Name, MethodBase.GetCurrentMethod().Name);
                var mirroredId = tr.GetMirroringReference();
                var tradeId = tr.getInternalCode();
                bool res = mirroredId > 0 && tradeId != mirroredId;
                logger.Write(CSMLog.eMVerbosity.M_debug, String.Format("Trade code = {0}, mirrored trade code = {1}. Is mirrored trade = {2}", tradeId, mirroredId, res));
                return res;
            }
        }

        #endregion
    }

}
