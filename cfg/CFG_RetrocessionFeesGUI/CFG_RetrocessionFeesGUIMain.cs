using System;

using sophis;
using sophis.utils;
using CFG_RetrocessionFeesGUI;
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace CFG_RetrocessionFeesGUI
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

            sophis.scenario.CSMScenario.Register("Retrocession Fees Results", new CFG_RetrocessionFeesGUI.CSxRetrocessionFeesResultsGUIScenario());
			//}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}