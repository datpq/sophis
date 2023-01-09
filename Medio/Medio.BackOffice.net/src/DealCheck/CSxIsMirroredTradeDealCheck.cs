using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using sophis.backoffice_kernel;
using sophis.portfolio;
using sophis.tools;
using sophis.utils;

namespace MEDIO.BackOffice.net.src.DealCondition
{
    public class CSxIsMirroredTradeDealCheck : CSMCheckDeal
    {
        public override void VoteForModification(CSMTransaction original, CSMTransaction transaction, int event_id)
        {
            if (!IsMirroredTrade(transaction))   
                throw new MTradeCheckingVoteException("Trade is not a mirrored trade ", 0, true);
        }

        public override void VoteForCreation(CSMTransaction transaction, int event_id)
        {
            if (!IsMirroredTrade(transaction))
                throw new MTradeCheckingVoteException("Trade is not a mirrored trade ", 0, true);
        }

        private bool IsMirroredTrade(sophis.portfolio.CSMTransaction tr)
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Begin(typeof(CSxIsMirroredTradeDealCheck).Name, MethodBase.GetCurrentMethod().Name);
                var mirroredId = tr.GetMirroringReference();
                var tradeId = tr.getInternalCode();
                bool res = mirroredId > 0 && tradeId != mirroredId;
                logger.Write(CSMLog.eMVerbosity.M_debug, String.Format("Trade code = {0}, mirrored trade code = {1}. Is mirrored trade = {2}", tradeId, mirroredId, res));
                return res;
            }
        }

    }
}
