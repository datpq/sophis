using System;

using sophis;
using sophis.utils;
using CFG_AccountClosing;
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace CFG_AccountClosing
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

            sophis.scenario.CSMScenario.Register("Accounting Closing", new CFG_AccountClosing.SourceFiles.AccountingClosingScenario());
			sophis.scenario.CSMScenario.Register("Accounting Opening", new CFG_AccountClosing.SourceFiles.AccountOpeningScenario());
			sophis.scenario.CSMScenario.Register("Accounting Result Distribution", new CFG_AccountClosing.SourceFiles.AffectationResultatScenario());
			//}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}