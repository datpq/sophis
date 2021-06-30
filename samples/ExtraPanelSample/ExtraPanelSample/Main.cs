using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis;
using sophis.oms.entry;
using sophis.instrument;
using sophis.amCommon;
using sophis.utils;

namespace ExtraPanelSample
{
    public class ExtraPanelSample : IMain
    {
        public void Close()
        {
        }

        public void EntryPoint()
        {
            try
            {
                EntryBoxExtraPanelManager.GetInstance().Register(new ExtraPanelUserControlKey(null, eMInstrumentType.M_iEquity, null, null), typeof(ExtraInformationsEquity));
            }
            catch(Exception exception)
            {
                AdvancedLogger.fLogger.Log("ExtraPanelSample", "EntryPoint", CSMLog.eMVerbosity.M_error,
                              "Loading assembly", "An internal error occurred when loading ExtraPanelSample assembly.",
                              exception, false);
            }
        }

    }
}
