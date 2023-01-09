using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using sophis;
using sophis.utils;
using sophis.instrument;


namespace MEDIO.BackOffice.net.src.DealAction
{
    public class CSxDealActionRBCABS : sophis.portfolio.CSMTransactionAction
    {
        string className = "CSxDealActionRBCABS";

        public override void VoteForCreation(sophis.portfolio.CSMTransaction transaction, int event_id)
        {
            // base.VoteForCreation(transaction, event_id);
            this.ActivateNativeLifeCycle();

            CSMInstrument instrument = transaction.GetInstrument();
            sophis.finance.CSMABSBond absBond = instrument;
            if (absBond != null)
            {
                CSMLog.Write(className, "VoteForCreation", CSMLog.eMVerbosity.M_debug, "Deal Creation on ABS checking for pool factor infos");
                double poolFactor = 1;
                CMString foInfos = transaction.GetComment();
                string[] foComments = foInfos.ToString().Split('|');

                if (foComments.Count() == 2 && foComments[1].Contains("PoolFactor"))
                {
                    CSMLog.Write(className, "VoteForCreation", CSMLog.eMVerbosity.M_debug, "Pool Factor infos found, retrieving value");
                    string[] pfInfos = foComments[1].Split('=');
                    poolFactor = Convert.ToDouble(pfInfos[1]);
                    if (poolFactor > 0)
                    {
                        transaction.SetPoolFactor(poolFactor);
                        double price = transaction.GetSpot();
                        CSMLog.Write(className, "VoteForCreation", CSMLog.eMVerbosity.M_debug, "Pool Factor = " + poolFactor.ToString());
                        CSMLog.Write(className, "VoteForCreation", CSMLog.eMVerbosity.M_debug, "removing the pool factor infos from comments ");
                        transaction.SetComment(foComments[0]);
                        transaction.SetSpot(price);
                        transaction.SetAccruedCoupon(0);
                        transaction.SetAccruedAmount(0.0);
                        transaction.SetAccruedAmount2(0.0);

                        transaction.RecomputeGrossAmount(false, false);

                        double netAmount = poolFactor * transaction.GetQuantity() * price / 100 + transaction.GetMarketFees() + transaction.GetBrokerFees() + transaction.GetCounterpartyFees();
                        transaction.SetNetAmount(netAmount);
                        transaction.SetPoolFactor(poolFactor);
                        

                    }
                }
            }


        }

        public override void NotifyCreated(sophis.portfolio.CSMTransaction transaction, sophis.tools.CSMEventVector message, int event_id)
        {



        }

    }


}
