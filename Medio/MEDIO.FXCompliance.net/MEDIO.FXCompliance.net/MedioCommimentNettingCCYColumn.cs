using sophis.utils;
using sophis.instrument;
using sophis.portfolio;
using sophis.backoffice_kernel;
using sophis.static_data;
using sophisTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Oracle.DataAccess.Client;
using System.Windows.Forms;
using sophis.market_data;
//using sophis.database;
//using Oracle.DataAccess.Client;

namespace MEDIO.FXCompliance.net
{
    public class MedioCommimentNettingCCYColumn : sophis.portfolio.CSMCachedPortfolioColumn
    {

        string sCCY = "";
        
        public string GetColumnName()
        {
            return MedioCustomParams.HedgeColumn + Utils.GetCCYName(this.sCCY) + " (net/hedge)";
        }

        public MedioCommimentNettingCCYColumn(string sCurrency)
        {
            this.sCCY = sCurrency;

            SetUseCache(true);
            SetInvalidateOnReporting(true);
            SetInvalidateOnCompute(true);
            SetInvalidateOnRefresh(false);
            SetDependsOnActivePortfolio(true);
        }

        public override void ComputePortfolioCell(SSMCellKey key, ref SSMCellValue cellValue, SSMCellStyle cellStyle)
        {
            if (key.Portfolio() == null || !key.Portfolio().IsLoaded()) return;

            //only for strategies and funds
            sophis.value.CSMAmPortfolio amFolio = sophis.value.CSMAmPortfolio.GetCSRPortfolio(key.PortfolioCode(), key.Extraction());
            if (amFolio == null)
                return;
            if (!amFolio.IsAStrategy() && !amFolio.IsPortfolioTrading())
                return;

            int currency = CSMCurrency.StringToCurrency(this.sCCY);

            // get the portfolio from its code and the extraction
            CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(key.PortfolioCode(), key.Extraction());
            CSMPortfolioColumn FXColumn = CSMPortfolioColumn.GetCSRPortfolioColumn(MedioCustomParams.FXColumn + Utils.GetCCYName(this.sCCY) + " (net)");
            CSMPortfolioColumn OtherColumn = CSMPortfolioColumn.GetCSRPortfolioColumn(MedioCustomParams.OtherColumn + Utils.GetCCYName(this.sCCY));

            if (portfolio == null || cellStyle == null || FXColumn == null || OtherColumn == null)
                return;

            SSMCellStyle style1 = new SSMCellStyle();
            SSMCellValue value1 = new SSMCellValue();
            SSMCellStyle style2 = new SSMCellStyle();
            SSMCellValue value2 = new SSMCellValue();

            FXColumn.GetPortfolioCell(key.ActivePortfolioCode(), key.PortfolioCode(), key.Extraction(), ref value1, style1, true);
            OtherColumn.GetPortfolioCell(key.ActivePortfolioCode(), key.PortfolioCode(), key.Extraction(), ref value2, style2, true);

            if (Math.Sign(value1.doubleValue) != Math.Sign(value2.doubleValue))
            {
                cellValue.doubleValue = Math.Abs(value1.doubleValue) - Math.Abs(value2.doubleValue);
                if (cellValue.doubleValue < 0)
                {
                    cellValue.doubleValue = 0;
                }
            }
            else
            {
                cellValue.doubleValue = Math.Abs(value1.doubleValue);
            }

            if (cellStyle != null)
            {
                // display value will be aligned to the right
                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                cellStyle.@decimal = 0;
                // display type when the value is null
                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                // set the font as bold
                cellStyle.style = sophis.gui.eMTextStyleType.M_tsNormal;


                cellStyle.currency = currency;
                var curr = CSMCurrency.GetCSRCurrency(currency);
                if (curr != null)
                    curr.GetRGBColor(cellStyle.color);

            }
        }

     }
}
