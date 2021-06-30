using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis;
using Sophis.Windows.OMS.Builder;
using sophis.oms;
using sophis.amCommon;
using sophis.utils;

namespace ExtraPanelRepoSample
{
    public class ExtraPanelRepoSample : IMain
    {
        public void Close()
        {
            
        }

        public void EntryPoint()
        {
            try
            {
                OrderEntryBuilderFactory.GetInstance().Register(OrderKind.Repo, new MyCustomRepoOrderBuilder());
            }
            catch (Exception exception)
            {
                AdvancedLogger.fLogger.Log("ExtraPanelSample", "EntryPoint", CSMLog.eMVerbosity.M_error,
                              "Loading assembly", "An internal error occurred when loading ExtraPanelSample assembly.",
                              exception, false);
            }
        }

    }
}
