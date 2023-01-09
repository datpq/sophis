using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Columns;
using sophis.OrderGeneration.AdjustExposure;
//using sophis.OrderGeneration.InstrumentAdjustment;
using sophis.utils;

namespace MEDIO.OrderAutomation.net
{
    class CSxDOBColumns : ColumnVisibilityHandler
    {
        public override bool IsColumnVisible(TreeListColumn column, int instrumentCode)
        {
            if (column == null)
                return false;

            if (String.IsNullOrEmpty(column.Name))
                return false;

            bool isBond = sophis.value.CSMAmValuationTools.IsInstrumentToBeHandledInNotional(instrumentCode);
            if (column.Name.Contains("Nominal"))
            {
                // Hide all Nominal Columns if not Bond
                if (!isBond)
                    return false;
                bool hasFloatingNotional = CSAMInstrumentDotNetTools.IsInstrumentFloatingNotional(instrumentCode);
                if (column.Name.Contains("Notional"))
                {
                    // Show Notional Column only if the Bond has Floating Notional (Inflation Bond, ABS Bond etc).
                    if (hasFloatingNotional)
                        return true;
                    return false;
                }
                // Hide Standard Nominal Columns if the Bond has Floating Notional.
                if (hasFloatingNotional)
                    return false;
            }
            else if (column.Name.Contains("Quantity"))
            {
                // Hide Quantity for Bonds.
                if (isBond)
                    return false;
            }
            return true;
        }
    }
}