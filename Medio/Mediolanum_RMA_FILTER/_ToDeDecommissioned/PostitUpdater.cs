using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichMarketAdapter.persistence;
using RichMarketAdapter.interfaces;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using sophis.utils;
using System.Globalization;
using sophisTools;
using RichMarketAdapter.ticket;

namespace Mediolanum_RMA_FILTER
{
    class PostitUpdater
    {
        public System.Timers.Timer aTimer;
        private static string sourceid = "RBCUploader";

        //TODO: initialize it in constructor
        public PostitUpdater(bool enable, int seconds,string sourceid)
        {
            aTimer = new System.Timers.Timer();
            aTimer.Interval = seconds * 1000;
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = enable;
            PostitUpdater.sourceid = sourceid;
        }

        public void EnableTimer()
        {
            aTimer.Enabled = true;
        }

        public void DisableTimer()
        {
            aTimer.Enabled = false;
        }

        public void SetTimer(int seconds)
        {
            aTimer.Interval = seconds * 1000;
        }

        public static void SetSourceID(string source_id)
        {
            PostitUpdater.sourceid = source_id;
        }

        public static string GetSourceID()
        {
            return PostitUpdater.sourceid;
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Dictionary<string, string> RMAMessages = new Dictionary<string, string>();

            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = "SELECT BINARYDATA,TEXTDATA FROM RMA_MESSAGES WHERE ENTEREDDATECODE = DATE_TO_NUM('" + DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "') AND LASTSTATUS IN (SELECT ID FROM RMA_STATUS WHERE NAME IN ('SuccessFirstCall','SuccessSecondCall')) AND SOURCEID = '" + sourceid + "'";
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] binaryData = (byte[])reader["BINARYDATA"];
                            string textData = (string)reader["TEXTDATA"];
                            IMessage message = MessageSerializer.Instance.deserialize(binaryData);
                            ITicketMessage ticketMessage = (ITicketMessage)message;
                            string extRef = ticketMessage.getString(transaction.FieldId.EXTERNALREF_PROPERTY_NAME);
                            if (!String.IsNullOrEmpty(extRef) && !String.IsNullOrEmpty(textData))
                            {
                                RMAMessages.Add(extRef, textData);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("PostitUpdater", "OnTimedEvent", CSMLog.eMVerbosity.M_error,
                    "Exception occurred while trying to get data from RMA_MESSAGES table: " + ex.Message + ". InnerException: " +
                    ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }
            StringBuilder strb = new StringBuilder("SELECT REFCON,INFOSBACKOFFICE FROM HISTOMVTS WHERE INFOSBACKOFFICE IS NOT NULL AND INFOSBACKOFFICE IN (");
            string[] extRefereces = RMAMessages.Keys.ToArray();
            for (int i = 0; i < extRefereces.Length; i++)
            {
                strb.Append("'");
                strb.Append(extRefereces[i]);
                strb.Append("'");
                if (i != extRefereces.Length - 1)
                {
                    strb.Append(",");
                }
            }
            strb.Append(")");

            Dictionary<int, string> trades = new Dictionary<int, string>();

            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = strb.ToString();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int refcon = Convert.ToInt32(reader["REFCON"]);
                            string extRef = (string)reader["INFOSBACKOFFICE"];
                            if (refcon != 0 && !String.IsNullOrEmpty(extRef))
                            {
                                trades.Add(refcon, extRef);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("PostitUpdater", "OnTimedEvent", CSMLog.eMVerbosity.M_error,
                    "Exception occurred while trying to get data from RMA_MESSAGES table: " + ex.Message + ". InnerException: " +
                    ex.InnerException + ". StackTrace: " + ex.StackTrace);
            }

            int[] tradeIDs = trades.Keys.ToArray();
            StringBuilder strb2 = new StringBuilder("SELECT MVTID,ACOMMENT FROM POSTIT WHERE TYPEID = 6 and MVTID IN (");
            for (int i = 0; i < tradeIDs.Length ; i++)
            {
                strb2.Append("'");
                strb2.Append(tradeIDs[i]);
                strb2.Append("'");
                if (i != tradeIDs.Length - 1)
                {
                    strb2.Append(",");
                }
            }
            strb2.Append(")");

            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = strb2.ToString();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int mvtid = Convert.ToInt32(reader["MVTID"]);
                            string comment = (string)reader["ACOMMENT"];
                            if (mvtid != 0 && !String.IsNullOrEmpty(comment))
                            {
                                if (comment.Equals(RMAMessages[trades[mvtid]]))
                                {
                                    trades.Remove(mvtid);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("PostitUpdater", "OnTimedEvent", CSMLog.eMVerbosity.M_error,
                    "Exception occurred while trying to get data from RMA_MESSAGES table: " + ex.Message + ". InnerException: " +
                    ex.InnerException + ". StackTrace: " + ex.StackTrace);
                return;
            }

            foreach (KeyValuePair<int, string> trade in trades)
            {
                try
                {
                    using (var cmd = new OracleCommand())
                    {
                        cmd.Connection = DBContext.Connection;
                        cmd.CommandText = "INSERT INTO POSTIT(ID,MVTID,USERID,ADATE,ATIME,ACOMMENT,TYPEID) VALUES((SELECT MAX(ID) FROM POSTIT)+1," + trade.Key + ",1,'" + DateTime.Now.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + "'," + CSMDay.GetSystemTime() + ",'" + RMAMessages[trade.Value] + "',6)";
                        int ret = cmd.ExecuteNonQuery();
                        CSMLog.Write("PostitUpdater", "OnTimedEvent", CSMLog.eMVerbosity.M_debug, "Insert query returned " + ret);
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write("PostitUpdater", "OnTimedEvent", CSMLog.eMVerbosity.M_error,
                        "Exception occurred while trying to insert data into POSTIT table: " + ex.Message + ". InnerException: " +
                        ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
        }
    }



}
