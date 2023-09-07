using NSREnums;
using sophis;
using sophis.finance;
using sophis.gui;
using sophis.instrument;
using sophis.portfolio;
using sophis.scenario;
using sophis.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEDIO.BackOffice.net
{
    class SwapPayLegDirtyPrice : CSMPortfolioColumn
    {

        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {

            try
            {
                double cellVal = 0.0;

                cellStyle.kind = eMDataType.M_dDouble;
                cellStyle.alignment = eMAlignmentType.M_aRight;              
                cellStyle.@decimal = 12;
                cellStyle.@null = eMNullValueType.M_nvNoNullValue;

                CSMSwap swap = CSMSwap.GetInstance(instrumentCode);
                if (swap != null)
                {
                    using (sophis.finance.CSMPricerSwap swapPricer = new CSMPricerSwap())
                    {
                     cellVal =100.0 * swapPricer.GetLegTheoreticalValue(swap, CSMMarketDataOverloader.GetCurrentMarketData(), 1);

                    }
                    
                }
                
                cellValue.doubleValue = cellVal;
            }
            catch (Exception ex)
            {
                sophis.utils.CSMLog.Write("SwapPayLegDirtyPrice", "GetPositionCell", CSMLog.eMVerbosity.M_error, "Exception Caught :" + ex.Message);
            }
        }

    }
}
