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
using sophis.misc;
using sophis.oms;
using sophis.portfolio;
using sophis.utils;
using sophis.value;
using Sophis.Data.Utils;

namespace MEDIO.OrderAutomation.net.Source.Criteria
{
    public class CSxMarketCapCriterium : CSMCriterium
    {
        private const string fClassName = "CSxParentOrderIDCriterium";
        private static string fColumnName = "Market Capitalization EUR";
        private static double firstCapLevel = 5000000000;
        private static double secondCapLevel = 15000000000;

        public CSxMarketCapCriterium(string columnName)
            : base()
        {
           
        }

        public CSxMarketCapCriterium()
            : base()
        {
            string levels = "";
            string colNameParam = "";
            CSMConfigurationFile.getEntryValue("MARKET_CAP_SECTION", "COLUMN_NAME", ref colNameParam, "Market Capitalization EUR");
            fColumnName = colNameParam;

            CSMConfigurationFile.getEntryValue("MARKET_CAP_SECTION", "BREACH_LEVELS", ref levels, "5000000000;15000000000");
            List<string> fields = levels.Split(';').ToList();
            double firstParam = 0;
            double secondParam = 0;
            if (fields.Count == 2 && Double.TryParse(fields[0], out firstParam) && Double.TryParse(fields[1], out secondParam))
            {
                firstCapLevel = firstParam;
                secondCapLevel = secondParam;
            }
        }

        public override CSMCriterium Clone()
        {
            return new CSxMarketCapCriterium(fColumnName);
        }

        public override CSMCriterium.MCriterionCaps GetCaps()
        {
            return new MCriterionCaps(false, true, false);
        }

        public override void GetCode(SSMReportingTrade mvt, ArrayList list)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                list.Clear();
                SSMOneValue value = new SSMOneValue();
                value.fCode = -1;
                SSMCellValue cellVal = new SSMCellValue();
                SSMCellStyle cellStyle = new SSMCellStyle();

                CSMPosition pos = CSMPosition.GetCSRPosition(mvt.mvtident, mvt.folio);
                if (pos != null)
                {

                    CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn(fColumnName);
                    if (col != null)
                    {
                        col.GetPositionCell(pos, pos.GetPortfolioCode(), pos.GetPortfolioCode(), pos.GetExtraction(), 0, pos.GetInstrumentCode(), ref cellVal, cellStyle, true);

                        double marketCap = 0;
                        marketCap = cellVal.doubleValue;

                        value.fCode = mvt.mvtident;
                        if (marketCap == 0)
                        {
                            value.fCode = -1;
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
                    }
                }

                list.Add(value);
                LOG.End();
            }
        }

        public override void GetName(int code, CMString name, long size)
        {
            if (code == -1)
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
