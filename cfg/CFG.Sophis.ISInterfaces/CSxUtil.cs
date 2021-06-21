using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using sophis.utils;
using sophis.market_data;
using sophisTools;

namespace CFG.Sophis.ISInterfaces
{
    class CSxUtil
    {
        static public OracleDataReader ExecuteReader(OracleCommand cmd)
        {
            CSMLog logger = new CSMLog();
            logger.Begin("CSxUtil", "ExecuteReader");

            logger.Write(CSMLog.eMVerbosity.M_debug, "Execute query : " + cmd.CommandText);
            OracleDataReader reader = cmd.ExecuteReader();
            logger.Write(CSMLog.eMVerbosity.M_debug, "Query executed successfully");

            logger.End();

            return reader;
        }

        static public void ExecuteNonQuery(OracleCommand cmd)
        {
            CSMLog logger = new CSMLog();
            logger.Begin("CSxUtil", "ExecuteNonQuery");

            logger.Write(CSMLog.eMVerbosity.M_debug, "Execute query : " + cmd.CommandText);
            cmd.ExecuteNonQuery();
            logger.Write(CSMLog.eMVerbosity.M_debug, "Query executed successfully");

            logger.End();
        }

        static public void SetPricesDate(int date)
        {
            CSMLog logger = new CSMLog();

            logger.Begin("CSxUtil", "SetPricesDate");

            CSMDay dateObj = new CSMDay(date);
            DateTime dateTime = new DateTime(dateObj.fYear, dateObj.fMonth, dateObj.fDay);
            logger.Write(CSMLog.eMVerbosity.M_debug, "Set prices date to " + dateTime.ToString("dd/MM/yyyy"));

            CSMMarketData.SSMDates ssDates = new CSMMarketData.SSMDates();

            ssDates.fCalculation = date;
            ssDates.fForex = date;
            //ssDates.fInstrument = date;
            ssDates.fPosition = date;
            ssDates.fRate = date;
            ssDates.fRepoCost = date;
            ssDates.fSpot = date;
            ssDates.fUseHistoricalFairValue = false;

            //DPH 733
            //ssDates.UseIt(true);
            ssDates.UseIt();

            logger.End();
        }
    }
}
