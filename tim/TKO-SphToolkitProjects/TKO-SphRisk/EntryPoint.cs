using System;
using sophis;
using sophis.portfolio;
using sophis.instrument;
using sophis.market_data;
using sophis.static_data;
using sophis.utils;
using TkoPortfolioColumn.CallBack;
using TkoPortfolioColumn.DbRequester;
using System.Threading;
using System.Configuration;
using sophis.collateral;

namespace TkoPortfolioColumn
{

    public class MainClass : sophis.IMain
    {
        public void EntryPoint()
        {
            try
            {
                PortFolioColumnCallbacker.Initialize();
            }
            catch (Exception ex)
            {
                CSMLog.Write("TkoPortfolioColumn", "EntryPoint", CSMLog.eMVerbosity.M_error, "Error while Loading Toolkit TKO-SophisToolkit-Columns [" + ex.Message + " ]");
            }
        }

        public void Close()
        {
            GC.Collect();
        }
    }
}
