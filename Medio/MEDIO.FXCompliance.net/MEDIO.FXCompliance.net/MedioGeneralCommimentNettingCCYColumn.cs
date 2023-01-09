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
    public class MedioGeneralCommimentNettingCCYColumn : sophis.portfolio.CSMCachedPortfolioColumn
    {
        public string GetColumnName()
        {
            //return "General Hedge Amount";
            return "Commitment - FX Forwards (net/hedge)";
        }

        public MedioGeneralCommimentNettingCCYColumn()
        {
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
            SSMCellStyle style = new SSMCellStyle();
            SSMCellValue value = new SSMCellValue();

            int currency = 0;
            // get the portfolio from its code and the extraction
            CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(key.PortfolioCode(), key.Extraction());

            if (portfolio == null || cellStyle == null)
            {
                return;
            }
            try
            {
                int nEURCCY = CSMCurrency.StringToCurrency("EUR");
                currency = nEURCCY;

                //currency
                for (int idx = 0; idx < CSMCurrency.GetCurrencyCount(); idx++)
                {
                    CSMCurrency curr = CSMCurrency.GetNthCurrency(idx);
                    int nCCY = curr.GetIdent();

                    if (nCCY != nEURCCY)
                    {
                        using (CMString sCCY = new CMString())
                        {
                            CSMCurrency.CurrencyToString(nCCY, sCCY);

                            CSMPortfolioColumn HedgeColumn = CSMPortfolioColumn.GetCSRPortfolioColumn(MedioCustomParams.HedgeColumn + Utils.GetCCYName(sCCY) + " (net/hedge)");
                            if (HedgeColumn != null)
                            {
                                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                                double fx = context.GetForex(nEURCCY, nCCY);

                                try
                                {
                                    HedgeColumn.GetPortfolioCell(key.ActivePortfolioCode(), key.PortfolioCode(), key.Extraction(), ref value, style, true);
                                    cellValue.doubleValue += value.doubleValue / fx;
                                }
                                //catch (Exception ex1)
                                catch
                                {
                                    //System.Windows.Forms.MessageBox.Show(ex.Message);
                                    //throw;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                //throw;
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
