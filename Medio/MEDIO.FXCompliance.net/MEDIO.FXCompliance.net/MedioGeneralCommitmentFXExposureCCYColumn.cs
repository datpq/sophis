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
    public class MedioGeneralCommitmentFXExposureCCYColumn : sophis.portfolio.CSMCachedPortfolioColumn
    {
        public string GetColumnName()
        {
            //return "General Commitment - FX Forwards";
            return "Commitment - FX Forwards (net)";
        }

        public MedioGeneralCommitmentFXExposureCCYColumn()
        {
            SetUseCache(true);
            SetInvalidateOnReporting(true);
            SetInvalidateOnCompute(true);
            SetInvalidateOnRefresh(false);
            SetDependsOnActivePortfolio(true);
        }

        public override void ComputePortfolioCell(SSMCellKey key, ref SSMCellValue cellValue, SSMCellStyle cellStyle)
        {
            int currency = 0;

            if (key.Portfolio() == null || !key.Portfolio().IsLoaded()) return;

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
                SSMCellStyle style = new SSMCellStyle();
                SSMCellValue value = new SSMCellValue();

                for (int idx = 0; idx < CSMCurrency.GetCurrencyCount(); idx++)
                {
                    CSMCurrency curr = CSMCurrency.GetNthCurrency(idx);
                    int nCCY = curr.GetIdent();

                    if (nCCY != nEURCCY)
                    {
                        using (CMString sCCY = new CMString())
                        {
                            CSMCurrency.CurrencyToString(nCCY, sCCY);

                            CSMPortfolioColumn FXColumn = CSMPortfolioColumn.GetCSRPortfolioColumn(MedioCustomParams.FXColumn + Utils.GetCCYName(sCCY) + " (net)");
                            if (FXColumn != null)
                            {
                                CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                                double fx = context.GetForex(nEURCCY, nCCY);

                                FXColumn.GetPortfolioCell(key.ActivePortfolioCode(), key.PortfolioCode(), key.Extraction(), ref value, style, true);
                                cellValue.doubleValue += Math.Abs(value.doubleValue / fx);
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

        public override void ComputePositionCell(SSMCellKey key, ref SSMCellValue cellValue, SSMCellStyle cellStyle)
        {
            try
            {
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Starting GetPositionCell");

                if (key.Portfolio() == null || !key.Portfolio().IsLoaded()) return;

                cellStyle.kind = NSREnums.eMDataType.M_dDouble;

                int currency = 0;

                try
                {
                    int nEURCCY = CSMCurrency.StringToCurrency("EUR");
                    currency = nEURCCY;
                    SSMCellStyle style = new SSMCellStyle();
                    SSMCellValue value = new SSMCellValue();

                    for (int idx = 0; idx < CSMCurrency.GetCurrencyCount(); idx++)
                    {
                        CSMCurrency curr = CSMCurrency.GetNthCurrency(idx);
                        int nCCY = curr.GetIdent();

                        if (nCCY != nEURCCY)
                        {
                            using (CMString sCCY = new CMString())
                            {
                                CSMCurrency.CurrencyToString(nCCY, sCCY);

                                CSMPortfolioColumn FXColumn = CSMPortfolioColumn.GetCSRPortfolioColumn(MedioCustomParams.FXColumn + Utils.GetCCYName(sCCY) + " (net)");
                                if (FXColumn != null)
                                {
                                    CSMMarketData context = CSMMarketData.GetCurrentMarketData();
                                    double fx = context.GetForex(nEURCCY, nCCY);

                                    FXColumn.GetPositionCell(key.Position(), key.ActivePortfolioCode(), key.PortfolioCode(), key.Extraction(), key.UnderlyingCode(), key.InstrumentCode(), ref value, style, true);
                                    cellValue.doubleValue += Math.Abs(value.doubleValue / fx);
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

                if (cellStyle!=null)
                {
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    cellStyle.currency = currency;
                    var curr = CSMCurrency.GetCSRCurrency(currency);
                    if (curr != null)
                        curr.GetRGBColor(cellStyle.color);
                }

                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Ending GetPositionCell");
            }
            catch (Exception ex)
            {
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_error, "Exception :" + ex.Message);
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Exception :" + ex.StackTrace);
            }

        }

     }


}
