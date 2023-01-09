using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sophis.portfolio;
using sophis.instrument;
using sophis.gui;
using sophis.static_data;
using sophis.market_data;
using Sophis.Logging;
using NSREnums;

namespace MEDIO.UserColumns.NET.Source.Column
{
    public class CSxLastMinus1 : CSMPortfolioColumn
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {

            cellStyle.@null = eMNullValueType.M_nvNoNullValue;
            cellStyle.kind = eMDataType.M_dDouble;
            cellStyle.@decimal = 2;
            cellStyle.style = eMTextStyleType.M_tsNormal;
            cellStyle.alignment = eMAlignmentType.M_aCenter;
            cellValue.SetString("");
            try
            {
                double lastMinus1 = CSxLastMinusN.getLastMinusN(instrumentCode, 1);
                cellValue.doubleValue = lastMinus1;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxLastMinus1::GetPositionCell error 1 : " + ex);
            }
        }
    }

    public class CSxLastMinus2 : CSMPortfolioColumn
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {

            cellStyle.@null = eMNullValueType.M_nvNoNullValue;
            cellStyle.kind = eMDataType.M_dDouble;
            cellStyle.@decimal = 2;
            cellStyle.style = eMTextStyleType.M_tsNormal;
            cellStyle.alignment = eMAlignmentType.M_aCenter;
            cellValue.SetString("");
            try
            {
                double lastMinus2 = CSxLastMinusN.getLastMinusN(instrumentCode, 2);
                cellValue.doubleValue = lastMinus2;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxLastMinus2::GetPositionCell error 1 : " + ex);
            }
        }
    }

    public class CSxLastMinusN
    {
        static public double getLastMinusN(int instCode, int n)
        {
            CSMInstrument inst = CSMInstrument.GetInstance(instCode);
            CSMOption opt = null;
            int tagLAST = 1279349588;       // value of the wide char 'LAST' in C++.
            int tagTHEO = 1414022479;       // 'THEO'
            int tag = tagLAST;
            if (inst != null && (opt=inst) != null && !opt.IsAListedOption())
                tag = tagTHEO;
            int foundDate = CSMMarketData.GetPositionDate();
            double last = 0.0;
            for (int i = 0; (i <= n) && (foundDate>0); ++i)
            {
                last = CSMHistoricalData.GetColumnValueFromHistory(instCode, foundDate, tag, ref foundDate);
                --foundDate;
            }
            return foundDate>0 ? last : 0.0;
        }
    }
}
