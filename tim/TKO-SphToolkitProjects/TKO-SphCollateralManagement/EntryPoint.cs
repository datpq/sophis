using System;

using sophis;
using sophis.utils;
using sophis.portfolio;
using sophis.collateral;

namespace TKO_SophisCollateralManagement
{
    /// <summary>
    /// Definition of DLL entry point, registrations, and closing point
    /// </summary>
    public class MainClass : IMain
    {
        public void EntryPoint()
        {
            sophis.collateral.CSMCollateralIndicator.Register("UnsettledBalance", new TkoCollateralIndicator());

            //Case => 01626016 waiting the wonderfull Sophis Support.....
            sophis.scenario.CSMScenario.Register("TikehauPushCollateralColumn", new TkoCollateralLimitCalculation());
        }

        public void Close()
        {
            GC.Collect();
        }
    }
}
