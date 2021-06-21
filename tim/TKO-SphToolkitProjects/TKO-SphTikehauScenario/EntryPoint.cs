using System;
using sophis.utils;
using sophis.scenario;

namespace TKOSphTikehauScenario
{
    public class MainClass : sophis.IMain
    {
        public void EntryPoint()
        {
            try
            {
                CSMScenario.Register("AAABondSynchroBBGClauseDate_GUI", new TkoSynchronizeNextCallDate());
            }
            catch (Exception ex)
            {
                CSMLog.Write("TkoScenario", "EntryPoint", CSMLog.eMVerbosity.M_error, string.Format("Error while Loading Toolkit TkoScenario [{0} ]", ex.Message));
            }
        }

        public void Close()
        {
            GC.Collect();
        }
    }
}
