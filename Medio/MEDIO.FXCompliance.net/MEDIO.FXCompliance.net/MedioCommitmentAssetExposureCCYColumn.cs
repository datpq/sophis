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
    public class MedioCommitmentAssetExposureCCYColumn : sophis.portfolio.CSMCachedPortfolioColumn
    {
        string sCCY = "";

        public string GetColumnName()
        {
            return MedioCustomParams.OtherColumn + Utils.GetCCYName(this.sCCY);
        }

        public MedioCommitmentAssetExposureCCYColumn(string sCurrency)
        {
            this.sCCY = sCurrency;
            fName = this.GetColumnName();

            SetUseCache(true);
            SetInvalidateOnReporting(true);
            SetInvalidateOnCompute(true);
            SetInvalidateOnRefresh(false);
            SetDependsOnActivePortfolio(true);
        }

        public override void ComputePortfolioCell(SSMCellKey key, ref SSMCellValue cellValue, SSMCellStyle cellStyle)
        {
            
            int currency = CSMCurrency.StringToCurrency(this.sCCY);

            //if (key.Portfolio() == null || !key.Portfolio().IsLoaded()) return;

            // get the portfolio from its code and the extraction
            CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(key.PortfolioCode(), key.Extraction());
            if (portfolio == null || cellStyle == null)
                return;

            CSMPosition position;
            int positionNumber = portfolio.GetTreeViewPositionCount();
            for (int index = 0; index < positionNumber; index++)
            {
                SSMCellStyle style = new SSMCellStyle();
                SSMCellValue value = new SSMCellValue();
                //SSMCellStyle style1 = new SSMCellStyle();
                //SSMCellValue value1 = new SSMCellValue();

                position = portfolio.GetNthTreeViewPosition(index);
                GetPositionCell(position, key.ActivePortfolioCode(), position.GetPortfolio().GetCode(), key.Extraction(), 0, position.GetInstrumentCode(), ref value, style, true);
                cellValue.doubleValue += value.doubleValue;
            }

            int nChildCount = portfolio.GetSiblingCount();
            if (nChildCount > 0)
            {
                for (int idx = 0; idx < nChildCount; idx++)
                {
                    //SSMCellStyle style = new SSMCellStyle();
                    //SSMCellValue value = new SSMCellValue();
                    SSMCellStyle style1 = new SSMCellStyle();
                    SSMCellValue value1 = new SSMCellValue();

                    CSMPortfolio directChild = portfolio.GetNthSibling(idx);


                    GetPortfolioCell(directChild.GetCode(), directChild.GetCode(), key.Extraction(), ref value1, style1, true);
                    cellValue.doubleValue += value1.doubleValue;
                }
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

                //if (key.Portfolio() == null || !key.Portfolio().IsLoaded()) return;

                cellStyle.kind = NSREnums.eMDataType.M_dDouble;

                double amount = 0;
                int currency = CSMCurrency.StringToCurrency(this.sCCY);
                //check if is FX Forward allotment
                CSMInstrument instr = CSMInstrument.GetInstance(key.InstrumentCode());
                string strType = instr.GetInstrumentType().ToString();

                SSMCellStyle styleR = new SSMCellStyle();
                SSMCellValue valueR = new SSMCellValue();

                CSMPortfolioColumn RankingsFXColumn = CSMPortfolioColumn.GetCSRPortfolioColumn("Classification Commitment type");
                if (RankingsFXColumn == null)
                {
                    return;
                }

                RankingsFXColumn.GetPositionCell(key.Position(), key.ActivePortfolioCode(), key.PortfolioCode(), key.Extraction(), key.UnderlyingCode(), key.InstrumentCode(), ref valueR, styleR, true); 
                
                //if ((strType != "E") && (strType != "X"))
                if (valueR.GetString() == MedioCustomParams.RankingOtherValue)
                {
                    List<int> ids = new List<int> { 1, 12, 13, 1159 };

                    int allotment = instr.GetAllotment();

                    //if (ids.IndexOf(instr.GetAllotment()) != -1)
                    {
                        int nCCY = instr.GetCurrency();
                        CMString CCY = new CMString();
                        CSMCurrency.CurrencyToString(nCCY, CCY);

                        if (CCY == this.sCCY)
                        {
                            int nEURCCY = CSMCurrency.StringToCurrency(this.sCCY);

                            SSMCellStyle style = new SSMCellStyle();
                            SSMCellValue value = new SSMCellValue();
                           // Original Code
                            //CSMPortfolioColumn.GetCSRPortfolioColumn("Asset Value").GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref value, style, true);
                            CSMPortfolioColumn.GetCSRPortfolioColumn("Market Value").GetPositionCell(key.Position(), key.ActivePortfolioCode(), key.PortfolioCode(), key.Extraction(), key.UnderlyingCode(), key.InstrumentCode(), ref value, style, true);
                            amount = value.doubleValue;
                        }
                    }
                }

                
                if (cellStyle != null)
                {
                    cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                    cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                    cellStyle.currency = currency;
                    var curr = CSMCurrency.GetCSRCurrency(currency);
                    if (curr != null)
                        curr.GetRGBColor(cellStyle.color);
                }
                

                cellValue.doubleValue = amount;//SetString(amount.ToString());

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
