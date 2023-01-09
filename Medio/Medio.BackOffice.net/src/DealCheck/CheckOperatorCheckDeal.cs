/*
** Includes
*/
using System;
using sophis;
using sophis.utils;
using sophis.portfolio;
using sophis.tools;
using sophis.oms;
using System.Collections.Generic;
using System.Reflection;

namespace MEDIO.BackOffice.net.src.DealCondition
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
    public class CheckOperatorCheckDeal : sophis.backoffice_kernel.CSMCheckDeal
	{

        public override void VoteForCreation(CSMTransaction transaction, int event_id)
        {
            UpdateUser(transaction);

        }
        
        public override void VoteForModification(CSMTransaction original, CSMTransaction transaction, int event_id)
        {
            UpdateUser(transaction);
        }

        private void UpdateUser(CSMTransaction transaction)
        {
            if (transaction.GetOrderId() <= 0)
            {
                CSMLog.Write("CheckOperatorCheckDeal", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "No order id associated du trade " + transaction.GetTransactionCode());
                return;
            }
            CSMLog.Write("CheckOperatorCheckDeal", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Order id = " + transaction.GetOrderId());

            bool found = false;

            //If there are performance issues we can use an SQL request
            //Get order version return the lines from order_audit sorted by timestamp asc/
            //TODO Migration - check the method below
            //IList<OrderAuditInfo> ordersAudit =
            //    OrderManagerConnector.Instance.GetOrderManager().GetOrderVersions(transaction.GetOrderId());
            IList<OrderAuditInfo> ordersAudit =
                OrderManagerConnector.Instance.GetOrderManager().GetOrderAudit(transaction.GetOrderId());
            foreach (OrderAuditInfo oneAudit in ordersAudit)
            {
                if (oneAudit.Reason == MEDIO.MEDIO_CUSTOM_PARAM.CSxToolkitCustomParameter.Instance.OMS_WF_OPERATORCHECK) //Default = "Send To Execute"
                {
                    int userId = oneAudit.User.UserID;
                    CSMLog.Write("CheckOperatorCheckDeal", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Transition with event = " + MEDIO.MEDIO_CUSTOM_PARAM.CSxToolkitCustomParameter.Instance.OMS_WF_OPERATORCHECK  + " detected for order Id " + transaction.GetOrderId() + " TradeId = " + transaction.GetTransactionCode() + " Trade operator changed to " + userId);
                    transaction.SetOperator(userId);
                    found = true;
                    //Do not break as we are looking for the last value (sorted by timestamp asc)
                }
            }
            if (found == false)
            {
                CSMLog.Write("CheckOperatorCheckDeal", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "No transition with event = " + MEDIO.MEDIO_CUSTOM_PARAM.CSxToolkitCustomParameter.Instance.OMS_WF_OPERATORCHECK + " detected for order Id " + transaction.GetOrderId() + " TradeId = " + transaction.GetTransactionCode());
            }
        }
        
    }
}
