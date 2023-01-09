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
    public class CSxLastDate : CSMPortfolioColumn
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();
        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {

            cellStyle.@null = eMNullValueType.M_nvNoNullValue;
            cellStyle.kind = eMDataType.M_dNullTerminatedString;
            cellStyle.style = eMTextStyleType.M_tsNormal;
            cellStyle.alignment = eMAlignmentType.M_aCenter;
            cellValue.SetString("");
            try
            {
                CSMInstrument inst = CSMInstrument.GetInstance(instrumentCode);
                CSMOption opt = inst;
                int tagLAST = 1279349588;       // value of the wide char 'LAST' in C++. No alternative to this horrid cludge. :-(
                int tagTHEO = 1414022479;       // 'THEO'
                int tag = tagLAST;
                if (opt != null && !opt.IsAListedOption())
                    tag = tagTHEO;
                int today = CSMMarketData.GetPositionDate();
                int foundDate = 0;
                double bingo = CSMHistoricalData.GetColumnValueFromHistory(instrumentCode, today, tag, ref foundDate);
                DateTime lastDate = new DateTime(1904, 01, 01, 0, 0, 0).AddDays(foundDate);
                cellValue.SetString(lastDate.ToShortDateString());
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxLastDate::GetPositionCell error 1 : " + ex);
            }
        }
    }
}
