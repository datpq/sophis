/*
** Includes
*/
using System;
using System.Text;
using sophis;
using sophis.scenario;
using sophis.utils;
using sophis.misc;
using System.Collections;
using System.Collections.Generic;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;
using sophis.instrument;
using System.Globalization;
using System.Threading;
using sophisTools;
using sophis.market_data;
using sophis.static_data;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace CFG.Sophis.ISInterfaces
{
	/// <summary>
    /// This class derived from <c>sophis.portfolio.CSMScenario</c> can be overloaded to create a new scenario
	/// </summary>
	public class CSxPriceHistorizerScenario : sophis.scenario.CSMScenario
	{           
        /// <summary>This method specifies the context in which it can be launched.</summary>
        /// <returns> Returns the type of the scenario. It returns <c>sophis.scenario.eMProcessingType.M_pScenario</c> by default.</returns>
        /// <remarks>
        /// <para>The various types of scenarios are: </para>
        /// <para>M_pScenario: Will be available through the Analysis menu when the porfolio is launched or when an instrument is opened.</para>
        /// <para>M_pManagerPreference: Will be available through the Manager menu without any condition.</para>
        /// <para>M_pUserPreference: Will be available through the User menu (or the Manager menu if available) without any condition.</para>
        /// <para>M_pInstrument: Will be available through the Data menu when an instrument is opened or selected.</para>
        /// <para>M_pCounterparty: Will be available through the Data menu when a third party is opened or selected.</para>
        /// <para>M_pPortfolio: Will be available through the Data menu when a folio is selected.</para>
        /// <para>M_pBeforeEndOfDayProcedure: Will be launched automatically before the end of day procedure.</para>
        /// <para>M_pAfterEndOfDayProcedure: Will be launched automatically after the end of day procedure.</para>
        /// <para>M_pNightBatch: Will be launched automatically in the night batch.</para>
        /// <para>M_pBeforeReporting: Will be launched automatically before every reporting.</para>
        /// <para>M_pAfterReporting: Will be launched automatically after every reporting.</para>
        /// <para>M_pMarketData: </para>
        /// <para>M_pData: </para>
        /// <para>M_pEndOfDayConditionnal: Add to the end of day in a conditional form.</para>
        /// <para>M_pAfterAllInitialisation: Will be executed after all initialiation.</para>
        /// <para>M_pOther: Will be added in the prototype but never used. May be used for a scenario on Calculation server.</para>
        /// <para>M_pMultiSiteEODBeforePortfolioLoading: Will be executed before the portfolio loading during a MultiSite End Of Day</para>
        /// <para>M_pAccounting: Will be available through the Accounting menu without any condition.</para>
        /// <para>M_pBalanceEngineBeforePnL: </para>
        /// <para>M_pBalanceEngineAfterPnL: </para>
        /// <para>M_pPNLEngine: </para>
        /// <para>M_pAuxiliaryLedger: </para>
        /// <para>M_pSendToGL: </para>
        /// </remarks>
        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pUserPreference;
        }        
        
        /// <summary>To run your scenario. this method is mandatory otherwise RISQUE will not do anything.</summary>
        public override void Run()
        {
            CSMLog logger = new CSMLog();
            logger.Begin("CSxPriceHistorizerScenario", "Run");

            try
            {
                //Get sensitivity user column name from .ini file
                string sensitivityUserColumn = "";
                CSMConfigurationFile.getEntryValue("CFG_SOPHIS_IS_INTERFACES", "SensitivityUserColumn", ref sensitivityUserColumn, "Sensibilité*");

                //Get historization date
                string historizationDateStr = "";
                CSMConfigurationFile.getEntryValue("CFG_SOPHIS_IS_INTERFACES", "HistorizationDate", ref historizationDateStr, "");
                DateTime historizationDateTime;                
                int historizationDate = CSMMarketData.GetCurrentMarketData().GetDate(); //By default take the sophis date

                if (DateTime.TryParseExact(historizationDateStr, "dd/MM/yyyy", new CultureInfo("fr-FR"), DateTimeStyles.None, out historizationDateTime) == true)
                {
                    CSMDay historizationDateObj = new CSMDay(historizationDateTime.Day, historizationDateTime.Month, historizationDateTime.Year);
                    historizationDate = historizationDateObj.toLong();
                }                

                //Get the list of instruments to historize
                List<SSxResults> listOfResults = GetListOfInstrumentsToHistorize();                

                //Get the list of instruments to update in HISTORIQUE table
                Collection<int> sicoToUpdateDico = GetListOfInstrumentsToUpdate(historizationDate);                
                                
                //Compute analytics
                ComputeAnalytics(listOfResults, historizationDate,sensitivityUserColumn);                   
             
                //Save results
                SaveResults(listOfResults, sicoToUpdateDico, historizationDate);                                
            }
            catch (Exception e)
            {               
                string mess = "An error occured while running \"Historize Prices\" scenario : " + e.ToString();
                logger.Write(CSMLog.eMVerbosity.M_error, mess);

                if (CSMApi.IsInBatchMode() == false)
                {
                    MessageBox.Show(mess);
                }
            }

            logger.End();
        }

        Collection<int> GetListOfInstrumentsToUpdate(int historizationDate)
        {
            CSMLog logger = new CSMLog();
            logger.Begin("CSxPriceHistorizerScenario", "GetListOfInstrumentsToUpdate");

            Collection<int> sicoToUpdateDico = new Collection<int>();

            string SQLQuery = "select T.SICOVAM from TITRES T, HISTORIQUE H where T.SICOVAM = H.SICOVAM and T.TYPE = 'O' and date_to_num(H.JOUR) = "
                                + historizationDate.ToString();

           

            using (OracleCommand myCommand = new OracleCommand(SQLQuery,DBContext.Connection))
            {
                using (OracleDataReader myReader = CSxUtil.ExecuteReader(myCommand))
                {
                    while (myReader.Read())
                    {
                        int sico = 0;
                        int.TryParse(myReader["SICOVAM"].ToString(), out sico);
                        sicoToUpdateDico.Add(sico);
                    }
                }
            }

            logger.End();

            return sicoToUpdateDico;
        }

        List<SSxResults> GetListOfInstrumentsToHistorize()
        {
            CSMLog logger = new CSMLog();
            logger.Begin("CSxPriceHistorizerScenario", "GetListOfInstrumentsToHistorize");
            
            List<SSxResults> listOfResults = new List<SSxResults>();
            
            string instrumentsWhereClause = "";
            CSMConfigurationFile.getEntryValue("CFG_SOPHIS_IS_INTERFACES", "HistorizationInstrumentsFilter", ref instrumentsWhereClause, "");

            string SQLQuery = "select SICOVAM from TITRES where TYPE = 'O'";

            if (instrumentsWhereClause != "")
            {
                SQLQuery += " and " + instrumentsWhereClause;
            }

            using (OracleCommand myCommand = new OracleCommand(SQLQuery, DBContext.Connection))
            {
                using (OracleDataReader myReader = CSxUtil.ExecuteReader(myCommand))
                {
                    while (myReader.Read())
                    {
                        SSxResults oneResults = new SSxResults();
                        int.TryParse(myReader["SICOVAM"].ToString(), out oneResults.fSicovam);
                        listOfResults.Add(oneResults);
                    }
                }
            }

            logger.End();
            return listOfResults;
        }

        void ComputeAnalytics(List<SSxResults> listOfResults, int historizationDate, string sensitivityUserColumn)
        {
            CSMLog logger = new CSMLog();

            logger.Begin("CSxPriceHistorizerScenario", "ComputeAnalytics");

            CSxBondPricerCLI bondPricer = new CSxBondPricerCLI();
            
            //Change Prices date if required
            int sophisDate = CSMMarketData.GetCurrentMarketData().GetDate();

            if (historizationDate != sophisDate)
            {
                CSxUtil.SetPricesDate(historizationDate);
            }
            
            foreach (SSxResults oneResult in listOfResults)
            {
                CSMBond bond = CSMInstrument.GetInstance(oneResult.fSicovam);
                if (bond != null)
                {
                    CSMBond bondClone = bond.Clone();

                    int settlementDate = bondClone.GetSettlementDate(historizationDate);
                    int ownershipDate = bondClone.GetPariPassuDate(historizationDate, settlementDate);
                    int accruedCouponDate = bondClone.GetAccruedCouponDate(historizationDate, settlementDate);
                    CSMDayCountBasis dayCountBasis = CSMDayCountBasis.GetCSRDayCountBasis(bondClone.GetMarketYTMDayCountBasisType());
	                CSMYieldCalculation yieldCalculation = CSMYieldCalculation.GetCSRYieldCalculation(bondClone.GetMarketYTMYieldCalculationType());
	                bool isAdjustedDatesCalc = bondClone.GetMarketCalculationYTMOnAdjustedDates();
	                bool isValueDatesCalc = bondClone.GetMarketCalculationYTMOnSettlementDate();
                    short adjustedDatesCalc = 0;
                    short valueDatesCalc = 0;
                    CSMMarketData currentMarketData = CSMMarketData.GetCurrentMarketData();
                    double dirtyPrice = 0;
                    double sensitivityStd = 0;
                    double convexity = 0;

                    if (isAdjustedDatesCalc == true)
                    {
                        adjustedDatesCalc = 1;
                    }

                    if (isValueDatesCalc == true)
                    {
                        valueDatesCalc = 1;
                    }

                    if (bondClone.GetPricingMethod() == eMPricingType.M_eptMtM_Greeks_MtM)
	                {
                        //DPH 733
                        //double mtmSpread = bondClone.ComputeMtMSpread();
                        double mtmSpread = CSMBond.ComputeMtMSpreadForPricing(bondClone);
                        bondClone.ResetMtMSpread(mtmSpread);
	                }	                
                    
                    //Price in amount
                    bondClone.SetQuotationType(eMAskQuotationType.M_aqInPrice);                    
                    bondClone.GetPriceDeltaGammaByZC(currentMarketData, ref oneResult.fPriceInAmount, ref sensitivityStd, ref convexity, historizationDate,
                                                        settlementDate, ownershipDate, adjustedDatesCalc, valueDatesCalc, dayCountBasis, yieldCalculation);                    

                    //Price in percent
                    bondClone.SetQuotationType(eMAskQuotationType.M_aqInPercentage);
                    bondClone.GetPriceDeltaGammaByZC(currentMarketData, ref oneResult.fPriceInPercent, ref sensitivityStd, ref convexity, historizationDate,
                                                        settlementDate, ownershipDate, adjustedDatesCalc, valueDatesCalc, dayCountBasis, yieldCalculation);

                    oneResult.fPriceInPercent -= bondClone.GetAccruedCoupon(ownershipDate, accruedCouponDate);

                    //In case of amortizing notional, one must adjust the price with the notional factor
                    double floatingNotionalFactor = bondClone.GetFloatingNotionalFactor(ownershipDate, accruedCouponDate);
                    double factor = 1;                    
                    if (bondClone.IsFloatingNotional())
                    {
                        if (floatingNotionalFactor > 1e-10)
                            factor = 1 / floatingNotionalFactor;
                        else
                            factor = 0;
                    }

                    oneResult.fPriceInPercent *= factor;

                    //Price in percent with accrued
                    bondClone.SetQuotationType(eMAskQuotationType.M_aqInPercentWithAccrued);                    
                    bondClone.GetPriceDeltaGammaByZC(currentMarketData, ref oneResult.fPriceInPercentWithAccrued, ref sensitivityStd, ref convexity, historizationDate,
                                                        settlementDate, ownershipDate, adjustedDatesCalc, valueDatesCalc, dayCountBasis, yieldCalculation);

                    oneResult.fPriceInPercentWithAccrued *= factor;

                    //YTM
                    bondClone.SetQuotationType(bond.GetQuotationType());
                    bondClone.GetPriceDeltaGammaByZC(currentMarketData, ref dirtyPrice, ref sensitivityStd, ref convexity, historizationDate,
                                                        settlementDate, ownershipDate, adjustedDatesCalc, valueDatesCalc, dayCountBasis, yieldCalculation);
                    oneResult.fYTM = bondClone.GetYTMByDirtyPrice(historizationDate, settlementDate, ownershipDate, dirtyPrice, adjustedDatesCalc, valueDatesCalc,
                                                                dayCountBasis, yieldCalculation, currentMarketData);
                    eMAskQuotationType e_quotationType = bondClone.GetAskQuotationType();
                    if (e_quotationType != eMAskQuotationType.M_aqInPrice && e_quotationType != eMAskQuotationType.M_aqInPriceWithoutAccrued && bondClone.IsFloatingNotional())
                    {
                        dirtyPrice *= factor;
                    }

                    //Accrued coupon in amount
                    bondClone.SetQuotationType(eMAskQuotationType.M_aqInPriceWithoutAccrued);
                    oneResult.fAccruedCouponInAmount = bondClone.GetAccruedCoupon(ownershipDate, accruedCouponDate);

                    //Accrued coupon in percent
                    bondClone.SetQuotationType(eMAskQuotationType.M_aqInPercentage);
                    oneResult.fAccruedCouponInPercent = bondClone.GetAccruedCoupon(ownershipDate, accruedCouponDate) * factor;
                    
                    //Duration                    
                    oneResult.fDuration = bondClone.GetDurationByZC(currentMarketData, historizationDate, settlementDate, ownershipDate, dayCountBasis,
                                                                    adjustedDatesCalc, valueDatesCalc, dayCountBasis, yieldCalculation);

                    //Sensitivity                    

                    oneResult.fSensitivity = bondPricer.GetSensitivity(bondClone, dirtyPrice);

                    bondClone.ResetMtMSpread();
                }
            }

            //Don't forget to restore Sophis date            
            if (historizationDate != sophisDate)
            {
                CSxUtil.SetPricesDate(sophisDate);
            }

            logger.End();
        }

        private void SaveResults(List<SSxResults> listOfResults, Collection<int> sicoToUpdateDico, int historizationDate)
        {
            CSMLog logger = new CSMLog();

            logger.Begin("CSxPriceHistorizerScenario","SaveResults");
            
            CultureInfo currentCultureInfo = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string sqlQuery = "";

            using (OracleCommand myCommand = new OracleCommand(sqlQuery, DBContext.Connection))
            {
                OracleTransaction transaction;
                transaction = DBContext.Connection.BeginTransaction();

                try
                {
                    foreach (SSxResults oneResult in listOfResults)
                    {
                        if (sicoToUpdateDico.Contains(oneResult.fSicovam) == true)
                        {
                            myCommand.CommandText = "update HISTORIQUE set CFG_THEO_IN_AMOUNT = " + oneResult.fPriceInAmount.ToString()
                                                    + ", CFG_THEO_IN_PERCENT = " + oneResult.fPriceInPercent.ToString()
                                                    + ", CFG_THEO_IN_PERC_WITH_ACCRUED = " + oneResult.fPriceInPercentWithAccrued.ToString()
                                                    + ", CFG_YTM = " + oneResult.fYTM.ToString()
                                                    + ", CFG_ACCRUED_IN_AMOUNT = " + oneResult.fAccruedCouponInAmount.ToString()
                                                    + ", CFG_ACCRUED_IN_PERCENT = " + oneResult.fAccruedCouponInPercent.ToString()
                                                    + ", CFG_DURATION = " + oneResult.fDuration.ToString()
                                                    + ", CFG_SENSITIVITY = " + oneResult.fSensitivity.ToString()
                                                    + " where SICOVAM = " + oneResult.fSicovam.ToString()
                                                    + " and date_to_num(JOUR) = " + historizationDate.ToString();
                        }
                        else
                        {
                            myCommand.CommandText = "insert into HISTORIQUE(SICOVAM,JOUR,CFG_THEO_IN_AMOUNT,CFG_THEO_IN_PERCENT,CFG_THEO_IN_PERC_WITH_ACCRUED"
                                                        + ",CFG_YTM,CFG_ACCRUED_IN_AMOUNT,CFG_ACCRUED_IN_PERCENT,CFG_DURATION,CFG_SENSITIVITY) values (" + oneResult.fSicovam.ToString()
                                                        + ",num_to_date(" + historizationDate.ToString() + ")"
                                                        + "," + oneResult.fPriceInAmount.ToString()
                                                        + "," + oneResult.fPriceInPercent.ToString()
                                                        + "," + oneResult.fPriceInPercentWithAccrued.ToString()
                                                        + "," + oneResult.fYTM.ToString()
                                                        + "," + oneResult.fAccruedCouponInAmount.ToString()
                                                        + "," + oneResult.fAccruedCouponInPercent.ToString()
                                                        + "," + oneResult.fDuration.ToString()
                                                        + "," + oneResult.fSensitivity.ToString()
                                                        + ")";
                        }

                        CSxUtil.ExecuteNonQuery(myCommand);
                    }
                                        
                    transaction.Commit();

                    logger.Write(CSMLog.eMVerbosity.M_debug, "Results saved successfully");
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    myCommand.Dispose();

                    string message = "An error occured when saving results (" + e.Message + ")";
                    logger.Write(CSMLog.eMVerbosity.M_error, message);

                    if (CSMApi.IsInBatchMode() == false)
                    {
                        MessageBox.Show(message);
                    }
                }
            }

            Thread.CurrentThread.CurrentCulture = currentCultureInfo;

            logger.End();
        }        
        
        class SSxResults
        {
            public int  fSicovam = 0;
            public double fPriceInAmount = 0;
            public double fPriceInAmountWithoutAccrued = 0;
            public double fPriceInPercent = 0;
            public double fPriceInPercentWithAccrued = 0;
            public double fYTM = 0;
            public double fAccruedCouponInAmount = 0;
            public double fAccruedCouponInPercent = 0;
            public double fDuration = 0;
            public double fSensitivity = 0;
        }
    }
}
