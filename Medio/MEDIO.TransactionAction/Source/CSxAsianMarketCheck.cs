/*
** Includes
*/
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis;
using sophis.utils;
using sophis.portfolio;
using System.ComponentModel;
using System.Globalization;
using sophis.tools;
using sophisTools;
using sophis.market_data;
using MEDIO.CORE.Tools;
using sophis.backoffice_kernel;
using sophis.instrument;
using Oracle.DataAccess.Client;
using sophis.static_data;
using sophis.misc;

namespace  MEDIO.TransactionAction.Source
{
    /// <remarks>
    /// <para>This class derived from <c>sophis.portfolio.CSMTransactionAction</c> is triggered when when saving transactions.</para>
    /// <para>You can overload this class and insert your own triggers.</para>   		
    /// <para>The list of triggers are called by methods like CSMTransaction::SaveToDatabasewhich modify transactions in the database.</para>
    /// <para>
    /// Before any save, an instance is created allowing you to save temporary data 
    /// in the vote with the insurance that the instance is on the same transaction during the notify.
    /// </para>
    /// </remarks>
    public class CSxAsianMarketCheck : sophis.portfolio.CSMTransactionAction
    {
        private const int eTradingHourCheck = MedioConstants.elementTradingHoursCheck;
        private const int eTradingHourChecklog = MedioConstants.elementTradingHoursCheckLog;
        public static int IntRateFutureAllotmentId = 0;
        /// <summary>votefor
        /// Ask for a creation of a transaction.
        /// When creating, first all the triggers will be called via VoteForCreation to check if they accept the
        /// creation in the order eMOrder + lexicographical order.
        /// The transaction ID can be  null; it will then created by the Sophis trigger.
        /// to interrupt creation throw <c>sophis.tools.MVoteException</c>
        /// </summary>
        /// <param name="transaction">transaction is the transaction to be created</param>
        /// <param name="event_id">BO kernel event ID</param>
        /// <exception cref="sophis.tools.MVoteException">if you reject that creation.</exception>
        /// <remarks>For compatibility reasons, by default call the version with one less parameter.</remarks>
        public override void NotifyCreated(CSMTransaction transaction, CSMEventVector message, int event_id)
        {
            using (CSMLog Log = new CSMLog())
            {
                Log.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                if (transaction == null)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, String.Format("Couldn't retrieve the transaction"));
                    Log.End(); return;
                }
                try
                {
                    if (IsFuture(transaction))
                    {

                        CSMConfigurationFile.getEntryValue("MEDIO.TransactionAction", "InterestRateFutureAllotId", ref IntRateFutureAllotmentId, 1203);

                        Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Transaction {0} is a future", transaction.getInternalCode()));
                        int date = transaction.GetTransactionDate();
                        Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Date {0}", date));
                        double time = transaction.GetTransactionTime();
                        Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Time {0}", time));
                        DateTime creationTime = CSxUtils.GetDateTimeFromSophisTime(date, time);
                        Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Creation time: ", creationTime.Date.ToLongDateString() + creationTime.ToLongTimeString()));
                        bool isAfterPit = IsTradingAfterPit(transaction.GetInstrumentCode(), creationTime);
                        // bool isBeforePit = IsTradingBeforePit(transaction.GetInstrumentCode(), creationTime);

                        if (isAfterPit /*|| isBeforePit*/)
                        {
                            Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Transaction {0} is after pit", transaction.getInternalCode()));
                            transaction.LoadUserElement();
                            transaction.SaveGeneralElement(eTradingHourCheck, true);
                            int nextBusDay = GetNextBusDay(date);
                            transaction.SetTransactionDate(nextBusDay);
                            string msg =
                                String.Format("The execution was received outside pit trading hours. Trade time was: {0} ",
                                    creationTime);
                            transaction.SaveGeneralElement(eTradingHourChecklog, msg);
                            transaction.SaveToDatabase(event_id);
                            Log.Write(CSMLog.eMVerbosity.M_debug, msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, String.Format("VoteForCreation exception! {0}", ex.Message));
                }
            }
        }

        public static bool IsTradingAfterPit(int sicaovam, DateTime orderTime)
        {
            bool res = false;
            using (CSMLog Log = new CSMLog())
            {
                string timezone = GetUnderlyingTimeZone(sicaovam);
                if (String.IsNullOrEmpty(timezone)) return res;
                string pitClosingTime = GetUnderlyingPitCloseTime(sicaovam);
                if (String.IsNullOrEmpty(pitClosingTime)) return res;

                DateTime pitDateTime = DateTime.Parse(pitClosingTime);
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                DateTime tradingTimezoneTime = TimeZoneInfo.ConvertTime(orderTime, timeZoneInfo);
                res = tradingTimezoneTime > pitDateTime ? true : false;
                Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Trading local time = {0}, converting to time zone '{1}' = {2}. Pit closing time = {3}. Is after pit session? {4} ", orderTime, timezone, tradingTimezoneTime, pitDateTime, res));
            }
            return res;
        }

        public static bool IsTradingBeforePit(int sicaovam, DateTime orderTime)
        {
            bool res = false;
            using (CSMLog Log = new CSMLog())
            {
                string timezone = GetUnderlyingTimeZone(sicaovam);
                if (String.IsNullOrEmpty(timezone)) return res;
                string pitOpeningTime = CSxAsianMarketCheck.GetUnderlyingPitOpenTime(sicaovam);
                if (String.IsNullOrEmpty(pitOpeningTime)) return res;

                DateTime pitDateTime = DateTime.Parse(pitOpeningTime);
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                DateTime tradingTimezoneTime = TimeZoneInfo.ConvertTime(orderTime, timeZoneInfo);
                res = tradingTimezoneTime < pitDateTime ? true : false;
                Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Trading local time = {0}, converting to time zone '{1}' = {2}. Pit closing time = {3}. Is after pit session? {4} ", orderTime, timezone, tradingTimezoneTime, pitDateTime, res));
            }
            return res;
        }

        private bool IsFuture(CSMTransaction transaction)
        {
            bool res = false;
            if (transaction.GetInstrument() != null)
            {
                if (transaction.GetInstrument().GetInstrumentType() == 'F')
                {
                    res = true;
                }
            }
            return res;
        }

        private static string GetUnderlyingPitCloseTime(int sicovam)
        {
            using (CSMLog Log = new CSMLog())
            {
                string res = "";
                SophisWcfConfig.SynchronizationContext.Send(delegate (object _)
                {
                    CSMFuture inst = CSMInstrument.GetInstance(sicovam);
                    if (inst != null)
                    {
                        int listMarketId = inst.GetListedMarketId();
                        int allotId = inst.GetAllotment();

                        if (listMarketId != 0 && allotId == IntRateFutureAllotmentId)
                        {

                            int optionId = 0;
                            string sMarketID = listMarketId.ToString();
                            string sSQL = "SELECT TEMPLATE FROM MO_ASSETCLASS WHERE POSINDEX=0 AND MARCHE=" + sMarketID;

                            try
                            {
                                using (OracleCommand cmd = Sophis.DataAccess.DBContext.Connection.CreateCommand())
                                {
                                    cmd.CommandText = sSQL;
                                    optionId = Convert.ToInt32(cmd.ExecuteScalar());
                                }
                            }
                            catch (Exception ex)
                            {
                                CSMLog.Write("CSxAsianMarketCheck", "GetUnderlyingPitCloseTime", CSMLog.eMVerbosity.M_error, "Exception Occured: " + ex.Message);

                            }
                            CSMOption listOption = CSMOption.GetInstance(optionId);
                            if (listOption != null)
                            {
                                int marketCode = listOption.GetMarketCode();

                                CSMMarket marketUnder = CSMMarket.GetCSRMarket(listOption.GetCurrencyCode(), marketCode);

                                if (marketUnder != null)
                                {
                                    res = marketUnder.GetExtRefValue("Pit closing time (local)");
                                }

                            }
                        }
                        else
                        {
                            int underlyingCode = inst.GetUnderlyingCode();
                            CSMInstrument underlying = CSMInstrument.GetInstance(underlyingCode);
                            if (underlying != null)
                            {
                                if (underlying.GetType_API() == 'A')
                                {

                                    int marketCode = underlying.GetMarketCode();

                                    CSMMarket marketUnder = CSMMarket.GetCSRMarket(underlying.GetCurrencyCode(), marketCode);

                                    if (marketUnder != null)
                                    {
                                        res = marketUnder.GetExtRefValue("Pit closing time (local)");
                                    }

                                }
                                else if (underlying.GetType_API() == 'I')
                                {
                                    res = CSxUtils.GetPitClosingTime(underlyingCode);
                                    Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Pit closing time retrived from underlying {0} = '{1}'", underlyingCode, res));

                                }

                            }
                            else
                            {
                                Log.Write(CSMLog.eMVerbosity.M_error, String.Format("Underlying of instrument {0} cannot be retrived! Underlying code = {1}", sicovam, underlyingCode));
                            }
                        }

                    }
                    else
                    {
                        Log.Write(CSMLog.eMVerbosity.M_error, String.Format("Instrument {0} cannot be retrived!", sicovam));
                    }
                }, null);
                return res;
            }
        }

        private static string GetUnderlyingPitOpenTime(int sicovam)
        {
            using (CSMLog Log = new CSMLog())
            {
                string res = "";
                SophisWcfConfig.SynchronizationContext.Send(delegate (object _)
                {
                    CSMFuture inst = CSMInstrument.GetInstance(sicovam);
                    if (inst != null)
                    {
                        int underlyingCode = inst.GetUnderlyingCode();
                        CSMInstrument underlying = CSMInstrument.GetInstance(underlyingCode);
                        if (underlying != null)
                        {
                            res = CSxUtils.GetPitOpeningTime(underlyingCode);
                            Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Pit opening time retrived from underlying {0} = '{1}'", underlyingCode, res));
                        }
                        else
                        {
                            Log.Write(CSMLog.eMVerbosity.M_error, String.Format("Underlying of instrument {0} cannot be retrived! Underlying code = {1}", sicovam, underlyingCode));
                        }
                    }
                    else
                    {
                        Log.Write(CSMLog.eMVerbosity.M_error, String.Format("Instrument {0} cannot be retrived!", sicovam));
                    }
                }, null);
                return res;
            }
        }

        private static string GetUnderlyingTimeZone(int sicovam)
        {
            using (CSMLog Log = new CSMLog())
            {
                string res = "";
                SophisWcfConfig.SynchronizationContext.Send(delegate (object _)
                {

                    CSMFuture inst = CSMInstrument.GetInstance(sicovam);
                    if (inst != null)
                    {

                        int listMarketId = inst.GetListedMarketId();
                        int allotId = inst.GetAllotment();

                        if (listMarketId != 0 && allotId == IntRateFutureAllotmentId)
                        {
                            int optionId = 0;
                            string sMarketID = listMarketId.ToString();
                            string sSQL = "SELECT TEMPLATE FROM MO_ASSETCLASS WHERE POSINDEX=0 AND MARCHE=" + sMarketID;

                            try
                            {
                                using (OracleCommand cmd = Sophis.DataAccess.DBContext.Connection.CreateCommand())
                                {
                                    cmd.CommandText = sSQL;
                                    optionId = Convert.ToInt32(cmd.ExecuteScalar());
                                }
                            }
                            catch (Exception ex)
                            {
                                CSMLog.Write("CSxAsianMarketCheck", "GetUnderlyingTimeZone", CSMLog.eMVerbosity.M_error, "Exception Occured: " + ex.Message);
                            }

                            CSMOption listOption = CSMOption.GetInstance(optionId);
                            if (listOption != null)
                            {
                                int marketCode = listOption.GetMarketCode();

                                CSMMarket marketUnder = CSMMarket.GetCSRMarket(listOption.GetCurrencyCode(), marketCode);

                                if (marketUnder != null)
                                {

                                    res = marketUnder.GetExtRefValue("Timezone");
                                    Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Market time zone retrived from underlying {0} = '{1}'", optionId, res));

                                }

                            }

                        }
                        else
                        {
                            int underlyingCode = inst.GetUnderlyingCode();
                            CSMInstrument underlying = CSMInstrument.GetInstance(underlyingCode);
                            if (underlying != null)
                            {
                                if (underlying.GetType_API() == 'A')
                                {

                                    int marketCode = underlying.GetMarketCode();

                                    CSMMarket marketUnder = CSMMarket.GetCSRMarket(underlying.GetCurrencyCode(), marketCode);

                                    if (marketUnder != null)
                                    {
                                        res = marketUnder.GetExtRefValue("Timezone");
                                        Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Market time zone retrived from underlying {0} = '{1}'", underlyingCode, res));

                                    }

                                }
                                else if (underlying.GetType_API() == 'I')
                                {
                                    res = CSxUtils.GeTimeZone(underlyingCode);
                                    Log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Market time zone retrived from underlying {0} = '{1}'", underlyingCode, res));
                                }

                            }
                            else
                            {
                                Log.Write(CSMLog.eMVerbosity.M_error, String.Format("Underlying of instrument {0} cannot be retrived! Underlying code = {1}", sicovam, underlyingCode));
                            }
                        }
                    }
                    else
                    {
                        Log.Write(CSMLog.eMVerbosity.M_error, String.Format("Instrument {0} cannot be retrived!", sicovam));
                    }
                }, null);
                return res;
            }

        }

        private int GetNextBusDay(int date)
        {
            CSMDay day = new CSMDay(date);
            do
            {
                day.addDay(1);
            } while (day.GetWeekday() == eMWeekdayType.M_wSaturday || day.GetWeekday() == eMWeekdayType.M_wSunday);
            return day.toLong();
        }
    }
}
