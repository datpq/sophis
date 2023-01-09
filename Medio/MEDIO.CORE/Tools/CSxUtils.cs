using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.utils;
using sophis.value;

namespace MEDIO.CORE.Tools
{
    public class CSxUtils
    {
        static readonly DateTime refdate = new DateTime(1904, 01, 01, 0, 0, 0);

        /// <summary>
        /// Get toolkit TITRES field MEDIO_PIT_CLOSING
        /// </summary>
        /// <param name="sicovam"></param>
        /// <returns></returns>
        public static string GetPitClosingTime(int sicovam)
        {
            string sql = String.Format("select MEDIO_PIT_CLOSING from titres where sicovam = {0}", sicovam);
            var res = Convert.ToString(CSxDBHelper.GetOneRecord(sql));
            return res;
        }

        /// <summary>
        /// Get toolkit TITRES field MEDIO_PIT_CLOSING
        /// </summary>
        /// <param name="sicovam"></param>
        /// <returns></returns>
        public static string GetPitOpeningTime(int sicovam)
        {
            string sql = String.Format("select MEDIO_PIT_OPENING from titres where sicovam = {0}", sicovam);
            var res = Convert.ToString(CSxDBHelper.GetOneRecord(sql));
            return res;
        }

        /// <summary>
        /// Get toolkit TITRES field MEDIO_MARKET_TIMEZONE
        /// </summary>
        /// <param name="sicovam"></param>
        /// <returns></returns>
        public static string GeTimeZone(int sicovam)
        {
            string sql = String.Format("select MEDIO_MARKET_TIMEZONE from titres where sicovam = {0}", sicovam);
            var res = Convert.ToString(CSxDBHelper.GetOneRecord(sql));
            return res;
        }

        public static DateTime GetDateTimeFromSophisTime(int date, double time)
        {
            DateTime dateTime = refdate.AddDays(date);
            return dateTime.AddSeconds(time);
            //string sql = String.Format("select NUMTIME_TO_DATE({0}, {1}) from dual", date, time);
            //var res = (CSxDBHelper.GetOneRecord(sql));
            //DateTime daetime = Convert.ToDateTime(res);
            //return daetime;
        }

        public static DateTime GetDateFromSophisTime(int date)
        {
            return refdate.AddDays(date);
            //string sql = String.Format("select NUM_TO_DATE({0}) from dual", date);
            //var res = (CSxDBHelper.GetOneRecord(sql));
            //DateTime daetime = Convert.ToDateTime(res);
            //return daetime;
        }

        public static int ToSophisDate(DateTime date)
        {
            TimeSpan diff = date - refdate.Date;
            return Convert.ToInt32(diff.TotalDays);
        }

        public static string GetFundName(int folioId)
        {
            CMString res = "";
            CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(folioId);
            if (folio != null)
            {
                CSMAmPortfolio fund = folio.GetFundRootPortfolio();
                if (fund != null)
                {
                    fund.GetName(res);
                }
            }
            return res;
        }
    }
}
