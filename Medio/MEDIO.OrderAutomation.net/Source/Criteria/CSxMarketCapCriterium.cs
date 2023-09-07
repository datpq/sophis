using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using MEDIO.OrderAutomation.net.Source.Data;
using Oracle.DataAccess.Client;
using sophis.instrument;
using sophis.market_data;
using sophis.misc;
using sophis.oms;
using sophis.portfolio;
using sophis.utils;
using sophis.value;
using Sophis.DailyData.Impl;
using sophisTools;
using Sophis.DailyData.DBAccess.Entities;


namespace MEDIO.OrderAutomation.net.Source.Criteria
{
    public class CSxMarketCapCriterium : CSMCriterium
    {
        private const string fClassName = "CSxParentOrderIDCriterium";
        private static string fDailyColumnName = "EQY_SH_OUT_REAL";
        private static int fDailyColumnId = 3;
        private static double firstCapLevel = 5000000000;
        private static double secondCapLevel = 15000000000;
        static CSMDailyDataMultisource _dailyDataSource=null;
        static DateTime _fusionDate= DateTime.Today;
        static int folioCcyEUR = 54875474;

        public CSxMarketCapCriterium(string columnName)
            : base()
        {
           
        }

        public CSxMarketCapCriterium()
            : base()
        {
            string levels = "";
            string colNameParam = "";
            int colId = 0;
            CSMConfigurationFile.getEntryValue("MARKET_CAP_SECTION", "DAILY_COLUMN_NAME", ref colNameParam, "EQY_SH_OUT_REAL");
            fDailyColumnName = colNameParam;

            CSMConfigurationFile.getEntryValue("MARKET_CAP_SECTION", "DAILY_COLUMN_ID", ref colId, 3);
            fDailyColumnId = colId;


            CSMConfigurationFile.getEntryValue("MARKET_CAP_SECTION", "BREACH_LEVELS", ref levels, "5000000000;15000000000");
            List<string> fields = levels.Split(';').ToList();
            double firstParam = 0;
            double secondParam = 0;
            if (fields.Count == 2 && Double.TryParse(fields[0], out firstParam) && Double.TryParse(fields[1], out secondParam))
            {
                firstCapLevel = firstParam;
                secondCapLevel = secondParam;
            }

            MULTISOURCE myS = new MULTISOURCE();
            myS.NAME = fDailyColumnName;
            myS.MULTISOURCEID = fDailyColumnId;
            myS.AGGREGATION_METHOD = "Portfolio Underlying";
            myS.DATATYPE = "Number";
            myS.PSET = "DAILYDATA";
            myS.DRT = false;
            myS.MODEL = "STANDARD";
            myS.INHERIT_FROM_ISSUER = false;
            myS.SOURCE = "NULL";
            myS.FIELD = "NULL";

            _dailyDataSource = new CSMDailyDataMultisource(myS);

        }

        public override CSMCriterium Clone()
        {
            return new CSxMarketCapCriterium();
        }

        public override CSMCriterium.MCriterionCaps GetCaps()
        {
            return new MCriterionCaps(true, true, false);
        }

        public override void GetCode(SSMReportingTrade mvt, ArrayList list)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                list.Clear();
                SSMOneValue value = new SSMOneValue();
                value.fCode = -1;

                int instrCode = mvt.sicovam;
                double? marketCap = 0;
                double fxRate = 1;

                int currentDate = sophis.market_data.CSMMarketData.GetCurrentMarketData().GetDate();
                using (CSMDay contextDate = new CSMDay(currentDate))
                {
                    _fusionDate = new DateTime(contextDate.fYear, contextDate.fMonth, contextDate.fDay);
                }
             
                        using (CSMInstrument posInstrument = CSMInstrument.GetInstance(instrCode))
                            { 
                            if (posInstrument != null)
                            {
                                double? nbOfShares = 0;
                                nbOfShares = posInstrument.GetInstrumentCount();
                                double lastFormula = 0;
                                lastFormula = posInstrument.GetDerivativeSpot(CSMMarketData.GetCurrentMarketData());
                               
                                    if (_dailyDataSource.IsValid())
                                    {
                                        SSMDailyData? dailyNb = Sophis.DailyData.Impl.CSMDailyDataExtraction.Instance.ExtractFirstAvailable(instrCode, _dailyDataSource, _fusionDate, 100);
                                        if (dailyNb != null)
                                        {
                                            if (dailyNb.Value.DataValue != null)
                                            {
                                                nbOfShares = dailyNb.Value.DataValue;
                                            }
                                        }

                                    }

                               
                                int instrCcy = posInstrument.GetCurrencyCode();
                              
                                if (instrCcy != 0 && folioCcyEUR != 0)
                                {
                                    fxRate = CSMMarketData.GetCurrentMarketData().GetForex(instrCcy, folioCcyEUR);
                                }

                                double lastFx = lastFormula * fxRate;
                                marketCap = nbOfShares * lastFx;

                                LOG.Write(CSMLog.eMVerbosity.M_debug, "Computed market capitalization(nbOfShares*last*fx) for instrument " + instrCode.ToString() + " is "+nbOfShares.ToString()+" * "+ lastFx.ToString()+" = "+ marketCap.ToString());
                            }

                        if (marketCap==null || marketCap == 0)
                        {
                            value.fCode = -100;
                        }
                        else if (marketCap < firstCapLevel)
                        {
                            value.fCode = 1;
                        }
                        else if (marketCap < secondCapLevel)
                        {
                            value.fCode = 2;
                        }
                        else
                        {
                            value.fCode = 3;
                        }
                        list.Add(value);
                        LOG.End();
                    }

            }
        }

        public override void GetName(int code, CMString name, long size)
        {
         
            if (code == -100)
            {
                name.StringValue = "N/A";
            }
            else if (code == 1)
            {
                name.StringValue = "Small Caps";
            }
            else if (code == 2)
            {
                name.StringValue = "Mid Caps";
            }
            else if (code == 3)
            {
                name.StringValue = "Large Caps";
            }

            size = name.StringValue.Length;
        }
    }
}
