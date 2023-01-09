using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sophis.portfolio;
using sophis.gui;
using NSREnums;

// this seems to be a duplicate of CSxFolioId

namespace MEDIO.UserColumns.NET.Source.Column
{
    public class CSxOpcvm : CSMPortfolioColumn
    {
        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode, CSMExtraction extraction, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            cellStyle.@null = eMNullValueType.M_nvNoNullValue;
            cellStyle.kind = eMDataType.M_dInt;
            cellStyle.style = eMTextStyleType.M_tsNormal;
            cellStyle.alignment = eMAlignmentType.M_aCenter;
            cellValue.integerValue = portfolioCode;
        }

        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {

            cellStyle.@null = eMNullValueType.M_nvNoNullValue;
            cellStyle.kind = eMDataType.M_dInt;
            cellStyle.style = eMTextStyleType.M_tsNormal;
            cellStyle.alignment = eMAlignmentType.M_aCenter;
            cellValue.integerValue = portfolioCode;
        }
    }
}
