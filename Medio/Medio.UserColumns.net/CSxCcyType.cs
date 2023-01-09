using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sophis.portfolio;
using sophis.instrument;
using sophis.gui;
using sophis.static_data;
using Sophis.Logging;
using NSREnums;

namespace MEDIO.UserColumns.NET.Source.Column
{
    public class CSxCcyType : CSMPortfolioColumn
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
                int ccy = inst.GetCurrency();
                // CSMCurrency currency = CSMCurrency.GetCSRCurrency(ccy);
                double fixedFx = 0.0;
                long majorCcy = CSMCurrency.GetBaseCurrency(ccy, ref fixedFx);
                cellValue.SetString(ccy == majorCcy ? "Big CCY" : "Small CCY");
            }
            catch (Exception ex)
            {
                _logger.LogDebug("CSxCcyType::GetPositionCell error 1 : " + ex);
            }
        }
    }
}
