using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using sophis.portfolio;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;

using TkoPortfolioColumn.DbRequester;
using sophis.scenario;


namespace TkoScenario
{
    public class TkoPushBloomberg : CSMScenario
    {

        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pNightBatch;
        }

        public override bool AlwaysEnabled()
        {
            return true;
        }

        public override void Run()
        {
            try
            {     
            }
            catch (Exception ex)
            {
            }
        }
    }
}
