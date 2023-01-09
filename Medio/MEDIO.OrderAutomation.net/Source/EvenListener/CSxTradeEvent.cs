/*
** Includes
*/
using System;
using sophis;
using sophis.utils;
using sophis.portfolio;
using MEDIO.OrderAutomation.NET.Source.GUI;
using sophis.instrument;
using System.Collections.Generic;
using sophis.xaml;
using sophis.tools;

namespace  MEDIO.OrderAutomation.net.Source.EvenListener
{
	/// <remarks>
    /// This class derived from <c>sophis.portfolio.CSMTransactionEvent</c> is triggered when another workstation has created, modified or deleted a transaction.
    /// </remarks>
	public class  CSxTradeEventFX : sophis.portfolio.CSMTransactionEvent
	{
        /// <summary>
        /// <para>This method is called when a transaction has been created in another workstation.</para>
		/// <para>When receiving an update event for a transaction, all triggers are called by this method
        /// in the order eMOrder + lexicographical order.
        /// </para>
        /// <para>Given that this new transaction event is defined for the listening workstation,
        /// this method is called only if the folio containing the transaction created is loaded.
        /// </para>
        /// </summary>
        /// <param name="transaction">is the new transaction.</param>
        /// <returns>
        /// false to bypass this event; in this case the other triggers are not executed and the window is not updated.
        /// Use this with moderation, only in the case M_oBefore.
        /// </returns>
        /// <seealso cref="sophis.portfolio.CSMTransactionEvent.eMOrder"/>
        public override bool HasBeenCreated(sophis.portfolio.CSMTransaction transaction)
        {
            DisableGUI(transaction);
            return true;
        }

        /// <summary>
        /// <para>This method is called when a transaction has been updated in another workstation.</para>
		/// <para>When receiving an update event for a transaction, all triggers are called by this method
        /// in the order eMOrder + lexicographical order.
        /// </para>
        /// <para>Given that this new transaction event is defined for the listening workstation,
        /// this method is called only if the folio containing the transaction created is loaded.
        /// </para>
        /// </summary>
        /// <param name="original">is the original version of the transaction.</param>
        /// <param name="transaction">is the modified version of the transaction.</param>
        /// <returns>
        /// false to bypass this event; in this case the other triggers are not executed and the window is not updated.
        /// Use this with moderation, only in the case M_oBefore.
        /// </returns>
        /// <seealso cref="sophis.portfolio.CSMTransactionEvent.eMOrder"/>
        public override bool HasBeenModified(sophis.portfolio.CSMTransaction original, sophis.portfolio.CSMTransaction transaction)
        {
            DisableGUI(transaction);
            return true;
        }

        /// <summary>
        /// <para>This method is called when a transaction has been deleted in a workstation.</para>
		/// <para>When receiving an update event for a transaction, all triggers are called by this method
        /// in the order eMOrder + lexicographical order.
        /// </para>
        /// <para>Given that this new transaction event is defined for the listening workstation,
        /// this method is called only if the folio containing the transaction created is loaded.
        /// </para>
        /// </summary>
        /// <param name="transaction">is the transaction before deletion.</param>
        /// <returns>
        /// false to bypass this event; in this case the other triggers are not executed and the window is not updated.
        /// Use this with moderation, only in the case M_oBefore.
        /// </returns>
        /// <seealso cref="sophis.portfolio.CSMTransactionEvent.eMOrder"/>
        public override bool HasBeenDeleted(sophis.portfolio.CSMTransaction transaction)
        {
            DisableGUI(transaction);
            return true;
        }

        internal static List<char> impactedInstruments = new List<char>{ 'X', 'E', 'K' };
        internal static void DisableGUI(sophis.portfolio.CSMTransaction transaction)
        {
            CSMLog.Write("CSxTradeEvent", "DisableGUI", CSMLog.eMVerbosity.M_debug, "Trade received " + transaction.GetTransactionCode() );
            try
            {
                if (CSxFXRollingForm.isOpened == false)
                {
                    return;
                }
                CSMLog.Write("CSxTradeEvent", "DisableGUI", CSMLog.eMVerbosity.M_debug, "GUI FX rolling Trade received " + transaction.GetTransactionCode());
            
                if( transaction.GetInstrument() != null )
                {
                    if( impactedInstruments.Contains(transaction.GetInstrument().GetInstrumentType() ) )
                    {
                         CSMLog.Write("CSxTradeEvent", "DisableGUI", CSMLog.eMVerbosity.M_debug, "GUI FX rolling disabled." );
                         XSRWinFormsAdapter<CSxFXRollingForm> adapter = XSRWinFormsAdapter<CSxFXRollingForm>.GetUniqueDialog("Medio FX Forward Roll", false);
                        (adapter.Control as CSxFXRollingForm).DisplayWarningAndDisableOrderRaising();
                    }
                }
            }
            catch(Exception ex)
            {
                CSMLog.Write("CSxTradeEvent", "DisableGUI", CSMLog.eMVerbosity.M_error, "Error on trade received " + transaction.GetTransactionCode() + " " + ex);
            }
        }
    }


    public class CSxTradeActionFX : sophis.portfolio.CSMTransactionAction
    {
        public override void NotifyCreated(CSMTransaction transaction, CSMEventVector message, int event_id)
        {
            CSxTradeEventFX.DisableGUI(transaction);
        }

        public override void NotifyDeleted(CSMTransaction transaction, CSMEventVector message, int event_id)
        {
            CSxTradeEventFX.DisableGUI(transaction);
        }

        public override void NotifyModified(CSMTransaction original, CSMTransaction transaction, CSMEventVector message, int event_id)
        {
            CSxTradeEventFX.DisableGUI(transaction);
        }
        
    }
}
