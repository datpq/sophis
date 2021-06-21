/*
** Includes
*/
using System;
using sophis;
using sophis.scenario;
using sophis.tools;
using sophis.utils;
using sophis.misc;

namespace CFG_Corporate_Data_Viewer
{
	/// <summary>
    /// This class derived from <c>sophis.portfolio.CSMScenario</c> can be overloaded to create a new scenario
	/// </summary>
	public class CFG_Corporate_Data_Viewer : sophis.scenario.CSMScenario
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

        /// <summary>To do all your initialisation. Typically, it may open a GUI to get data from the user.</summary>
        public override void Initialise()
        {
            /// Add your code here
        }

        /// <summary>To run your scenario. this method is mandatory otherwise RISQUE will not do anything.</summary>
        public override void Run()
        {
            /// Add your code here
            sophis.gui.WinformContainer.DoDialog(new CFG_Corporate_Data_GUI(), sophis.gui.WinformContainer.eDialogMode.eMDI);
        }

        /// <summary>Free initiliased memory after scenario is processed.</summary>
        public override void Done()
        {
            /// Add your code here
        }
    }
}
