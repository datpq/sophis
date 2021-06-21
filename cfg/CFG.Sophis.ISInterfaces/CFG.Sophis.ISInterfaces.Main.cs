using System;

using sophis;
using sophis.utils;
using sophis.instrument;
using CFG.Sophis.ISInterfaces;
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)
using sophis.reporting;

//}}SOPHIS_TOOLKIT_INCLUDE


namespace CFG.Sophis.ISInterfaces
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

            sophis.scenario.CSMScenario.Register("Price Historizer", new CFG.Sophis.ISInterfaces.CSxPriceHistorizerScenario());
            CSMXMLSource.Register("CFG_ComputeTheoretical", new CSxComputeTheoreticalXMLSource());
            CSMXMLSource.Register("CFG_GetPriceFromYtm", new CSxGetPriceFromYtmXMLSource());
            CSMXMLSource.Register("CFG_GetYtmFromPrice", new CSxGetYtmFromPriceXMLSource());
            CSMXMLSource.Register("CFG_GetYieldCurve", new CSxGetYieldCurveXMLSource());

			//}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}