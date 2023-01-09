using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.OrderAutomation.NET.Source.OrderCreationValidator;
using NSREnums;
using sophis.gui;
using sophis.instrument;
using Sophis.OMS.Util;
using sophis.portfolio;
using sophis.utils;

namespace MEDIO.OrderAutomation.NET.Source.Column
{

    /// <summary>
    /// Beware of the impact on the performance! It's not a good idea to do such a column (although used for compliance checks only)
    /// </summary>
    public class CSxOrderSameSidePortfolioColumn :CSMPortfolioColumn
    {
        //public override void ComputePositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode,
        //    CSMExtraction extraction,
        //    int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle,
        //    bool onlyTheValue)
        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            using (CSMLog Log = new CSMLog())
            {
                Log.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                cellStyle.kind = eMDataType.M_dLong;
                cellValue.integerValue = 0;
                if (position.GetPositionType() != eMPositionType.M_pStandard)
                {
                    var orders = CSxOrderCreationBuySellValidator.GetOrdersFromPositions(position);
                    if (orders.IsNullOrEmpty()) return;
                    bool sameSide = orders.Select(x => x.Side).Distinct().Count() == 1;
                    //cellValue.SetString(sameSide ? "Yes" : "No");
                    cellValue.integerValue = sameSide ? 1 : 0;
                }
                Log.End();
            }
        }
    }
}
