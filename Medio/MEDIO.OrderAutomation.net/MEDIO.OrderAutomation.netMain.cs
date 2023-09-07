using System;

using sophis;
using MEDIO.OrderAutomation.net.Source.Criteria;
using MEDIO.OrderAutomation.NET.Source.Column;
using MEDIO.OrderAutomation.NET.Source.Criteria;
using MEDIO.OrderAutomation.NET.Source.OrderCreationValidator;
using MEDIO.OrderAutomation.NET.Source.Scenarios;
using sophis.oms;
using MEDIO.OrderAutomation.NET.Source.GUI;

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace MEDIO.OrderAutomation.net
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
            
                // GUI
                sophis.scenario.CSMScenario.Register("Medio Hedging & Funding Blotter", new MEDIO.OrderAutomation.net.Source.Scenarios.CSxHedgingFundingOrdersScenario());
                sophis.scenario.CSMScenario.Register("Medio FX Forward Roll", new MEDIO.OrderAutomation.net.Source.Scenarios.CSxForwardRollingScenario());
                sophis.scenario.CSMScenario.Register("Medio Instrument Adjustment", new CSxDynamicOrderBuilderMAML());
                CSMUserRights user = new CSMUserRights();
                user.LoadDetails();
                if (user.HasAccess("FX Automation"))
                {
                    sophis.scenario.CSMScenario.Register("Medio FX Automation", new Source.Scenarios.CSxFXAutomationScenario());
                }
                if (user.HasAccess("FX Automation Setup"))
                {
                    sophis.scenario.CSMScenario.Register("Medio FX Automation Setup", new Source.Scenarios.CSxFXAutomationSettingScenario());
                }
                sophis.scenario.CSMScenario.Register("Medio FX Adjustment", new CSxDynamicOrderBuilderMAMLFX());//TO CHANGE
                sophis.portfolio.CSMPositionCtxMenu.Register("Medio Instrument Adjustment", new CSxDynamicOrderBuilderMAMLCtx());
                sophis.portfolio.CSMPositionCtxMenu.Register("Medio FX Adjustment", new CSxDynamicOrderBuilderMAMLFXCtx());

                //DOB Columns

                CSxDOBColumns Columns = new CSxDOBColumns();
                sophis.OrderGeneration.AdjustExposure.ColumnVisibilityHandlerContainer.RegisterHandler(Columns);

                // Extraction
                CSxHedgingFundingCriterium.InitOrderSicovamList();
                CSxHedgingFundingCriterium.InitTradeIDParentOrderList();
                sophis.portfolio.CSMCriterium.Register(@"Hedging/Funding", new CSxHedgingFundingCriterium());
                sophis.portfolio.CSMCriterium.Register(@"FO Asset class - Hedging/Funding", new CSxFOAssetClassCriterium());
                sophis.portfolio.CSMCriterium.Register(@"Parent Order ID", new CSxParentOrderIDCriterium());
				sophis.portfolio.CSMCriterium.Register(@"MEDIO Lookthrough", new CSxLookthroughCriterium());
                sophis.portfolio.CSMCriterium.Register(@"MEDIO Market Capitalization", new CSxMarketCapCriterium());

            // Order Validation 
            OrderCreationValidatorManager.Instance.Register(typeof(CSxOrderCreationFolioValidator), new CSxOrderCreationFolioValidator());
                OrderCreationValidatorManager.Instance.Register(typeof(CSxOrderCreationThirdpartyValidator), new CSxOrderCreationThirdpartyValidator());
                OrderCreationValidatorManager.Instance.Register(typeof(CSxOrderCreationShortSellValidator), new CSxOrderCreationShortSellValidator());
                OrderCreationValidatorManager.Instance.Register(typeof(CSxOrderCreationAssignationValidator), new CSxOrderCreationAssignationValidator());
               // OrderCreationValidatorManager.Instance.Register(typeof(CSxOrderCreationFXValidator), new CSxOrderCreationFXValidator());
                OrderCreationValidatorManager.Instance.Register(typeof(CSxOrderCreationMiscValidator), new CSxOrderCreationMiscValidator());
                OrderCreationValidatorManager.Instance.Register(typeof(CSxOrderCreationBuySellValidator), new CSxOrderCreationBuySellValidator());
                OrderCreationValidatorManager.Instance.Register(typeof(CSxOrderCreationBondValidator), new CSxOrderCreationBondValidator());

                // Event 
             //   CSxDOBEventListener.Install();

                // Folio Column
                sophis.portfolio.CSMPortfolioColumn.Register("Orders Same Side", new CSxOrderSameSidePortfolioColumn());
                sophis.portfolio.CSMPortfolioColumn.Register("Rebalancing Constraint", new CSxRebalancingConstraint());
            
            
                sophis.portfolio.CSMTransactionEvent.Register("CSxTradeEventFX", sophis.portfolio.CSMTransactionEvent.eMOrder.M_oAfter, new MEDIO.OrderAutomation.net.Source.EvenListener.CSxTradeEventFX());
                sophis.portfolio.CSMTransactionAction.Register("CSxTradeActionFX", sophis.portfolio.CSMTransactionAction.eMOrder.M_oAfterSophisValidation, new MEDIO.OrderAutomation.net.Source.EvenListener.CSxTradeActionFX());

                sophis.portfolio.CSMPositionCtxMenu.Register("MEDIO Performance YtD", new CSxPerfDashboardMenu(0));
                sophis.portfolio.CSMPositionCtxMenu.Register("MEDIO Performance MtD", new CSxPerfDashboardMenu(1));
                sophis.portfolio.CSMPositionCtxMenu.Register("MEDIO Performance QtD", new CSxPerfDashboardMenu(2));

            //}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}