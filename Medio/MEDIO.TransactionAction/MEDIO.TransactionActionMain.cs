using System;

using sophis;
using sophis.utils;
using MEDIO.TransactionAction;
///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace MEDIO.TransactionAction
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

            sophis.portfolio.CSMTransactionAction.Register("CSxAsianMarketCheck", sophis.portfolio.CSMTransactionAction.eMOrder.M_oBeforeSophisValidation, new MEDIO.TransactionAction.Source.CSxAsianMarketCheck());
            sophis.portfolio.CSMTransactionAction.Register("CSxBondAccruedCheck", sophis.portfolio.CSMTransactionAction.eMOrder.M_oBeforeSophisValidation, new MEDIO.TransactionAction.Source.CSxBondAccruedCheck());

            sophis.portfolio.CSMTransactionAction.Register("CSxFundSettleCtpyCheck", sophis.portfolio.CSMTransactionAction.eMOrder.M_oAfterSophisValidation, new MEDIO.TransactionAction.Source.CSxFundSettleCtpyCheck());
            sophis.portfolio.CSMTransactionAction.Register("CSxRMAGenericTradeCheck", sophis.portfolio.CSMTransactionAction.eMOrder.M_oBeforeSophisValidation, new MEDIO.TransactionAction.Source.CSxRMAGenericTradeCheck());
            //}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }

}