using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NSREnums;
using sophis.gui;
using sophis.instrument;
using sophis.portfolio;
using sophis.utils;

namespace MEDIO.NestedRule.net
{


    public class CSxLevlFirstAgregg : CSMPortfolioColumn
    {
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode, sophis.portfolio.CSMExtraction extraction, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            double retval = 0.0; //double for compatibility with compliance
            try
            {
                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio.GetSiblingCount() == 0)
                    retval = 1;

                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.kind = NSREnums.eMDataType.M_dDouble; 
                cellStyle.@decimal = 0;
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsNormal;

                cellValue.doubleValue = retval;

            }
            catch (Exception ex)
            {
                sophis.utils.CSMLog.Write("CSxLEvelFirstAggreg", "GetPortfolioCell", CSMLog.eMVerbosity.M_error, "Exception Caught :" + ex.Message);
            }
        }

    }
}
