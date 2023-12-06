using System;
using System.Linq;
using sophis;
using sophis.utils;
using sophis.portfolio;
using Medio.BackOffice.net;
using MEDIO.BackOffice.net.src.Allotment;
using MEDIO.BackOffice.net.src.Scenario;
using MEDIO.BackOffice.net.src.DealCondition;
using MEDIO.BackOffice.net.src.KernelEngine;
using MEDIO.BackOffice.net.src.Thirdparty;
using sophis.backoffice_kernel;
using MEDIO.BackOffice.net.src.DealAction;
using MEDIO.BackOffice.net;

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace Medio.BackOffice.net
{
    /// <summary>
    /// Definition of DLL entry point, registrations, and closing point
    /// </summary>
    public class MainClass : IMain
    {
        public void EntryPoint()
        {
            //{{SOPHIS_INITIALIZATION (do not delete this line)

            // TO DO; Perform registrations
            try
            {
                // Transaction Action for ABS
                CSMTransactionAction.Register("CSxDealActionRBCABS", CSMTransactionAction.eMOrder.M_oSavingInDataBase, new CSxDealActionRBCABS());

                //GUI
                sophis.scenario.CSMScenario.Register("MedioStrategyUpdate", new CSxStrategyFilter());
                // Kernel Engine 
                CSMKernelEngine.Register("Medio Email Notification", new CSxEmailKernelEngine());
                CSMKernelEngine.Register("Medio Execution Avg Price", new CSxExecutionAveragePriceKernelEngine());
                CSMKernelEngine.Register("Medio Fees Refresh", new CSxRefreshFeesKernelEngine());
                CSMKernelEngine.Register("Medio Check Avg Price + Send to BBH", new CSxSendToBBHKernelEngine());



                CSMThirdPartyAction.Register("Update Thirdparty cache", CSMThirdPartyAction.eMOrder.M_oSave, new CSxThirdpartyAction());
                foreach (var one in CSxThirdpartyCondition.GetAgreementNames().Distinct())
                    sophis.backoffice_kernel.CSMKernelCondition.Register(String.Format("Valid {0}", one), new CSxThirdpartyCondition(one));

                CSxThirdpartySettlementCondition.InitializeCache();
                // Settlement place 
                foreach (string settlePlace in CSxThirdpartySettlementCondition.GetCache().Select(x => x.SettlePlaceCode).Where(x => !String.IsNullOrEmpty(x)).Distinct())
                    sophis.backoffice_cash.CSMSettlementRulesCondition.Register("Is " + settlePlace, new CSxThirdpartySettlementCondition(settlePlace, eConditionTpe.settlePlace));

                //// Country
                foreach (string country in CSxThirdpartySettlementCondition.GetCache().Select(x => x.Country).Where(x => !String.IsNullOrEmpty(x)).Distinct())
                    sophis.backoffice_cash.CSMSettlementRulesCondition.Register("Is " + country, new CSxThirdpartySettlementCondition(country, eConditionTpe.country));

                // Allotment conditions
                CSxGenericAllotmentCondition.InitConditions();
                foreach (var model in CSxGenericAllotmentCondition.GetModels())
                    CSMAllotmentCondition.Register(model.ConditionName, new CSxGenericAllotmentCondition(model));
 

                CSMCheckDeal.Register("Check Operator", new CheckOperatorCheckDeal());
                CSMCheckDeal.Register("Check FXALL Trade ID", new CheckFXALLExternalReference());
                CSMCheckDeal.Register("Check Average Price", new CheckExecutionAveragePrice());

                sophis.instrument.CSMInstrumentAction.Register("CSxBondNotionalCheck", sophis.instrument.CSMInstrumentAction.eMOrder.M_oModification, new CSxBondNotionalCheck());
                
				  sophis.instrument.CSMInstrumentAction.Register("CSxRussianBondRedemption", sophis.instrument.CSMInstrumentAction.eMOrder.M_oCreation, new CSxRussianBondRedemption());
               

                sophis.instrument.CSMInstrumentAction.Register("CSxAbsPoolFactorCheck", sophis.instrument.CSMInstrumentAction.eMOrder.M_oModification, new CSxAbsPoolFactorCheck());

                sophis.portfolio.CSMPortfolioColumn.Register("MEDIO Pay DirtyPrice", new SwapPayLegDirtyPrice());
                sophis.portfolio.CSMPortfolioColumn.Register("MEDIO Rec DirtyPrice", new SwapRecLegDirtyPrice());
            }
            catch(Exception ex)
            {
                CSMLog.Write("MainClass","EntryPoint", CSMLog.eMVerbosity.M_error, "Error in entry point:" + ex.Message);
                CSMLog.Write("MainClass","EntryPoint", CSMLog.eMVerbosity.M_error, "Inner Exception:" + ex.InnerException);
                CSMLog.Write("MainClass","EntryPoint", CSMLog.eMVerbosity.M_error, "Stack Trace:" + ex.StackTrace);
            }

            //}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}