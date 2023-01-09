/*
** Includes
*/
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis;
using sophis.utils;
using sophis.portfolio;
using System.ComponentModel;
using System.Globalization;
using sophis.tools;
using sophisTools;
using sophis.market_data;
using MEDIO.CORE.Tools;
using sophis.backoffice_kernel;
using sophis.instrument;


namespace MEDIO.TransactionAction.Source
{
    /// <remarks>
    /// <para>This class derived from <c>sophis.portfolio.CSMTransactionAction</c> is triggered when when saving transactions.</para>
    /// <para>You can overload this class and insert your own triggers.</para>   		
    /// <para>The list of triggers are called by methods like CSMTransaction::SaveToDatabasewhich modify transactions in the database.</para>
    /// <para>
    /// Before any save, an instance is created allowing you to save temporary data 
    /// in the vote with the insurance that the instance is on the same transaction during the notify.
    /// </para>
    /// </remarks>
    public class CSxBondAccruedCheck : sophis.portfolio.CSMTransactionAction
    {

        public override void VoteForCreation(CSMTransaction transaction, int event_id)
        {
            using (CSMLog Log = new CSMLog())
            {
                try
                {
                    CSMBond bond = transaction.GetInstrument();

                    if (bond != null)
                    {
                        Log.Write(CSMLog.eMVerbosity.M_info, "Transaction to be created on a bond, checking if accrued in same direction as quantity");
                        double qty = transaction.GetQuantity();
                        double accrued = transaction.GetAccruedAmount();
                        
                        //if (transaction.GetPaymentCurrencyType() == CSMTransaction.eMPaymentCurrencyType.M_pcSettlement)
                        //{
                        //    double fx = transaction.GetForexSpot();
                        //    accrued *= fx;
                        //}

                        Log.Write(CSMLog.eMVerbosity.M_info, " Deal Quantity = " + qty + " Deal Accrued = " + accrued);

                        if (qty * accrued < 0)
                        {
                            Log.Write(CSMLog.eMVerbosity.M_info, "Opposite direction modiying accrued to its opposite");
                            transaction.SetAccruedAmount(-accrued);
                            Log.Write(CSMLog.eMVerbosity.M_info, "New Accrued: " + transaction.GetAccruedAmount());
                        }

                    }
                }
                catch (Exception ex)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, String.Format("Exception Occured: "+ex.Message));
                }
            }
        }
    }
}