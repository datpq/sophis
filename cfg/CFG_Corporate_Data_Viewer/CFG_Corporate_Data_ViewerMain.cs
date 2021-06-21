using System;

using sophis;
using sophis.utils;
using CFG_Corporate_Data_Viewer;
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace CFG_Corporate_Data_Viewer
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

            sophis.scenario.CSMScenario.Register("CFG Corporate Data Viewer", new CFG_Corporate_Data_Viewer());
			//}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}