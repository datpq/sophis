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

namespace MEDIO.FXCompliance.net
{
    public class MedioCommitmentFXExposureCCYColumn : sophis.portfolio.CSMCachedPortfolioColumn
    {
        string sCCY = "";
        string CachingFXExposureCCYColumn = "";

        Dictionary<int, double> columnCache = new Dictionary<int, double>();
        Dictionary<string, double> fxForwardCache = new Dictionary<string, double>();
        //int refreshVersion = -1;

        public string GetColumnName()
        {
            return MedioCustomParams.FXColumn + Utils.GetCCYName(this.sCCY) + " (net)";
        }

        public MedioCommitmentFXExposureCCYColumn(string sCurrency, string sCachingFXExposureCCYColumn)
        {
            this.sCCY = sCurrency;
            this.CachingFXExposureCCYColumn = sCachingFXExposureCCYColumn;
            columnCache.Clear();
            fxForwardCache.Clear();

            SetUseCache(true);
            SetInvalidateOnReporting(true);
            SetInvalidateOnCompute(true);
            SetInvalidateOnRefresh(false);
            SetDependsOnActivePortfolio(true);
        }

        public override void ComputePortfolioCell(SSMCellKey key, ref SSMCellValue cellValue, SSMCellStyle cellStyle)
        {
            SSMCellStyle style = new SSMCellStyle();
            SSMCellValue value = new SSMCellValue();
            SSMCellStyle style1 = new SSMCellStyle();
            SSMCellValue value1 = new SSMCellValue();
            int currency = CSMCurrency.StringToCurrency(this.sCCY);

            if (key.Portfolio() == null || !key.Portfolio().IsLoaded()) return;

            // get the portfolio from its code and the extraction
            CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(key.PortfolioCode(), key.Extraction());
            if (portfolio == null || cellStyle == null)
                return;

            CSMPosition position;
            int positionNumber = portfolio.GetTreeViewPositionCount();
            for (int index = 0; index < positionNumber; index++)
            {
                position = portfolio.GetNthTreeViewPosition(index);

                CMString instName = position.GetCSRInstrument().GetName();

                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                   MethodBase.GetCurrentMethod().Name,
                   CSMLog.eMVerbosity.M_debug, " In Portfolio : " + portfolio.GetName().ToString() + " , Position ID : " + position.GetIdentifier().ToString() + " , On Instrument : "+instName.ToString());

                GetPositionCell(position, key.ActivePortfolioCode(), position.GetPortfolio().GetCode(), key.Extraction(), 0, position.GetInstrumentCode(), ref value, style, true);

                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                   MethodBase.GetCurrentMethod().Name,
                   CSMLog.eMVerbosity.M_debug, " Retieved Value : " + value.doubleValue);

                
                cellValue.doubleValue += value.doubleValue;
            }

            int nChildCount = portfolio.GetSiblingCount();
            if (nChildCount > 0)
            {
                for (int idx = 0; idx < nChildCount; idx++)
                {
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

            if ( CachingFXExposureCCYColumn == "N" )
                columnCache.Clear(); 
        }

        public override void ComputePositionCell(SSMCellKey key, ref SSMCellValue cellValue, SSMCellStyle cellStyle)
        {
            try
            {

                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_comm, "Starting GetPositionCell");

                if (key.Portfolio() == null || !key.Portfolio().IsLoaded()) return;

                cellStyle.kind = NSREnums.eMDataType.M_dDouble;

                double amount = 0;

                //implementing a cache for optim and compliance calls.
                // Key on Position ID

                int posId = key.Position().GetIdentifier();
                double posVal = 0.0;
                
                // Debug Info Logs
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                MethodBase.GetCurrentMethod().Name,
                 CSMLog.eMVerbosity.M_debug, "Position ID : " + posId + " , Refreshversion : " + MedioCustomParams.gRefreshVersion);

                if (MedioCustomParams.gRefreshVersion != sophis.portfolio.CSMPortfolioColumn.GetRefreshVersion())
                {
                    CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                MethodBase.GetCurrentMethod().Name,
                 CSMLog.eMVerbosity.M_info, GetColumnName() + " Refreshing, Old Version :" + MedioCustomParams.gRefreshVersion);
                    MedioCustomParams.gRefreshVersion = sophis.portfolio.CSMPortfolioColumn.GetRefreshVersion();
                    CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
             MethodBase.GetCurrentMethod().Name,
              CSMLog.eMVerbosity.M_info,  " New Version :" + MedioCustomParams.gRefreshVersion);
                    columnCache.Clear();
                }

                if (columnCache.TryGetValue(posId, out posVal) == false)
                {

                    // Debug Info Logs
                    CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                     CSMLog.eMVerbosity.M_debug, "Data Not in Cache, computing value.");

                    int currency = CSMCurrency.StringToCurrency(this.sCCY);
                    //check if is FX Forward allotment
                    CSMInstrument instr = CSMInstrument.GetInstance(key.InstrumentCode());
                    string strType = instr.GetInstrumentType().ToString();

                    SSMCellStyle styleR = new SSMCellStyle();
                    SSMCellValue valueR = new SSMCellValue();

                    //should be static (unless it's cached...)
                    CSMPortfolioColumn RankingsFXColumn = CSMPortfolioColumn.GetCSRPortfolioColumn(MedioCustomParams.RankingColumnName);
                    if (RankingsFXColumn == null)
                    {
                        return;
                    }

                    RankingsFXColumn.GetPositionCell(key.Position(), key.ActivePortfolioCode(), key.PortfolioCode(), key.Extraction(), key.UnderlyingCode(), key.InstrumentCode(), ref valueR, styleR, true);

                    //if ((strType == "E") || (strType == "X"))
                    if (valueR.GetString() == MedioCustomParams.RankingFXValue)
                    {
                        CSMForexFuture forexFuture = CSMForexSpot.GetInstance(instr.GetCode());
                        CSMForexSpot forexSpot = CSMForexSpot.GetInstance(instr.GetCode());
                        SSMFxPair fxPair = new SSMFxPair();

                        if (forexFuture != null)
                        {
                            fxPair.fDev1 = forexFuture.GetCurrency();
                            fxPair.fDev2 = forexFuture.GetExpiryCurrency();
                        }
                        else
                        {
                            if (forexSpot != null)
                            {
                                fxPair = forexSpot.GetFxPair();
                            }
                            else
                            {
                                return;
                            }
                        }

                        int nCCY1 = fxPair.fDev1;
                        int nCCY2 = fxPair.fDev2;
                        CMString CCY1 = new CMString();
                        CSMCurrency.CurrencyToString(nCCY1, CCY1);
                        CMString CCY2 = new CMString();
                        CSMCurrency.CurrencyToString(nCCY2, CCY2);

                        if ((CCY1 == this.sCCY) || (CCY2 == this.sCCY))
                        {
                            // Getting info for logs ...
                            // instruemtn Name
                            CMString instrumentName = new CMString("");
                            CSMInstrument inst = key.Position().GetCSRInstrument();
                            if (inst != null)
                                instrumentName = inst.GetName();

                            // Position Transactions 
                            CSMTransactionVector transaction_list = new CSMTransactionVector();
                         //   position.GetTransactions(transaction_list); THIS CALL IN COMPLIANCE RETRIEVES POSITION FROM ANOTHER POSITION...
                            // FORCING THE TRANSACTION LIST...
                            CSMPosition pos = CSMPosition.GetCSRPosition(posId, key.Extraction(), key.PortfolioCode());
                            pos.GetTransactions(transaction_list);
                            int transactionCount = transaction_list.Count;
                            double transactionAmount = 0.0;
                            foreach (CSMTransaction t in transaction_list)
                            {
                                // TODO check if BE is valid...
                                transactionAmount += t.GetNetAmount();
                            }

                            CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                            MethodBase.GetCurrentMethod().Name,
                            CSMLog.eMVerbosity.M_debug, "Portfolio : " + key.PortfolioCode() + "Checking Position #: " + key.Position().GetIdentifier().ToString() + " , " + instrumentName.ToString() + " , Number of Securities : " + key.Position().GetInstrumentCount().ToString() + " , Trade Count : " + transactionCount + " , Amount : " + transactionAmount);

                            if (CCY1 != this.sCCY)
                            {
                                //SSMCellStyle style = new SSMCellStyle();
                                //SSMCellValue value = new SSMCellValue();
                                //CSMPortfolioColumn.GetCSRPortfolioColumn(MedioCustomParams.FXAmount1).GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref value, style, true);
                                //amount = value.doubleValue;
                                amount += key.Position().GetInstrumentCount();
                                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                                MethodBase.GetCurrentMethod().Name,
                               CSMLog.eMVerbosity.M_debug, " Retrieved Nominal 1st CCY : " + amount.ToString());
                            }
                            if (CCY2 != this.sCCY)
                            {
                                //SSMCellStyle style = new SSMCellStyle();
                                //SSMCellValue value = new SSMCellValue();
                                //CSMPortfolioColumn.GetCSRPortfolioColumn(MedioCustomParams.FXAmount2).GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref value, style, true);
                                //amount = value.doubleValue;
                                amount += -transactionAmount;
                                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                                MethodBase.GetCurrentMethod().Name,
                                CSMLog.eMVerbosity.M_debug, " Retrieved Nominal 2nd CCY : " + amount.ToString());
                            }

                            // Tricking the Compliance GUI assuming as observed:
                            // The first call is made properly to GetPosition Cell and retrieve proper data
                            // The second call is done by GetPortfolio with "new" position ID for FX Forwrd hence no data from trades can be retrieved.
                            if (amount != 0)
                            {
                               // CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                               // if (folio != null)
                                {
                               //     CMString folioName = folio.GetName();
                                    int prevRefresh = MedioCustomParams.gRefreshVersion - 1;
                                    string fxFrwrdKey = key.InstrumentCode().ToString() + "_" + MedioCustomParams.gRefreshVersion.ToString();
                                    string fxFrwrdPrevKey = key.InstrumentCode().ToString() + "_" + prevRefresh.ToString();
                                    // adding new entry for cache
                                    CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                                    MethodBase.GetCurrentMethod().Name,
                                    CSMLog.eMVerbosity.M_debug, " Adding new entry to fx frwrd cache with key : "+fxFrwrdKey+" , value : "+amount.ToString());
                                    fxForwardCache.Add(fxFrwrdKey, amount);
                                    // removing previous one since the dictionary
                                    if(fxForwardCache.Keys.Contains(fxFrwrdPrevKey))
                                        fxForwardCache.Remove(fxFrwrdPrevKey);

                                }
                            }
                            else
                            {
                               // CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                               // if (folio != null)
                                {
                                  //  CMString folioName = folio.GetName();
                                    int prevRefresh = MedioCustomParams.gRefreshVersion - 1;
                                    string fxFrwrdKey = key.InstrumentCode().ToString() + "_" + prevRefresh.ToString();

                                    CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                                    MethodBase.GetCurrentMethod().Name,
                                    CSMLog.eMVerbosity.M_debug, " Checking fx frwrd cache with key : " + fxFrwrdKey);

                                    if (fxForwardCache.Keys.Contains(fxFrwrdKey))
                                    {

                                        amount = fxForwardCache[fxFrwrdKey];
                                        CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                                        MethodBase.GetCurrentMethod().Name,
                                        CSMLog.eMVerbosity.M_debug, " Found Value : " + amount.ToString());
                                    }
                                }

                            }
                        }
                    }

                    /*
                    if (!onlyTheValue)
                    {
                        cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                        cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                        cellStyle.currency = currency;
                        var curr = CSMCurrency.GetCSRCurrency(currency);
                        if (curr != null)
                            curr.GetRGBColor(cellStyle.color);
                    }
                    */

                    columnCache[posId] = amount;

                }

                cellValue.doubleValue = columnCache[posId];
                long mem = System.Environment.WorkingSet;
                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
                    MethodBase.GetCurrentMethod().Name,
                    CSMLog.eMVerbosity.M_debug, "Memory usage : [" + mem + "]");

                CSMLog.Write(MethodBase.GetCurrentMethod().DeclaringType.Name,
    MethodBase.GetCurrentMethod().Name,
    CSMLog.eMVerbosity.M_debug, "ColumnCache Size: : " + columnCache.Count + " Forward Cache Size: " + fxForwardCache.Count + " Column: " + GetColumnName()); 
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
