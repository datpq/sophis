/*
** Includes
*/
using System;
using sophis;
using sophis.utils;
using sophis.portfolio;
using sophis.oms;
using sophis.tools;
using System.Reflection;
using Oracle.DataAccess.Client;
using System.Collections.Generic;
using System.Linq;

namespace  MEDIO.TransactionAction.Source
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
	public class  CSxFundSettleCtpyCheck : sophis.portfolio.CSMTransactionAction
	{
        /// <summary>votefor
        /// Ask for a creation of a transaction.
        /// When creating, first all the triggers will be called via VoteForCreation to check if they accept the
        /// creation in the order eMOrder + lexicographical order.
        /// The transaction ID can be  null; it will then created by the Sophis trigger.
        /// to interrupt creation throw <c>sophis.tools.MVoteException</c>
        /// </summary>
        /// <param name="transaction">transaction is the transaction to be created</param>
        /// <param name="event_id">BO kernel event ID</param>
        /// <exception cref="sophis.tools.MVoteException">if you reject that creation.</exception>
        /// <remarks>For compatibility reasons, by default call the version with one less parameter.</remarks>
        public override void VoteForCreation(sophis.portfolio.CSMTransaction transaction, int event_id)
        {
            using (CSMLog Log = new CSMLog())
            {
                try
                {
                    Log.Begin(GetType().Name, MethodBase.GetCurrentMethod().Name);
                    Log.Write(CSMLog.eMVerbosity.M_debug, $"BEGIN. TradeId={transaction.getInternalCode()}, OrderId={transaction.GetOrderId()}");
                    if (transaction.GetOrderId() == 0)
                    {
                        Log.Write(CSMLog.eMVerbosity.M_debug, "OrderId = 0. Nothing to do.");
                        return;
                    }
                    var order = OrderManagerConnector.Instance.GetOrderManager().GetOrderById(transaction.GetOrderId());
                    Log.Write(CSMLog.eMVerbosity.M_debug, $"ExternalSystem={order.ExternalSystem}, GetAllotment={transaction.GetInstrument().GetAllotment()}");
                    foreach (var eac in getExternalSystemAllotmentCtpy())
                    {
                        var parts = eac.Split(',');
                        var externalSystem = parts[0];
                        var allotment = int.Parse(parts[1]);
                        var ctpy = int.Parse(parts[2]);
                        if (order.ExternalSystem == externalSystem && transaction.GetInstrument().GetAllotment() == allotment)
                        {
                            Log.Write(CSMLog.eMVerbosity.M_debug, $"eac={eac}");
                            transaction.SetCounterparty(ctpy);
                            updateExecutionCounterparty(transaction.GetOrderId(), ctpy);
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(CSMLog.eMVerbosity.M_error, $"Exception Occured: {ex.Message}");
                }
                finally
                {
                    Log.Write(CSMLog.eMVerbosity.M_debug, "END.");
                }
            }
        }

        private void updateExecutionCounterparty(int orderId, int ctpy) {
            CSMLog.Write("CSxFundSettleCtpyCheck", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, $"orderId={orderId}, SetCounterparty to {ctpy}");
            try
            {
                using (OracleCommand cmd = Sophis.DataAccess.DBContext.Connection.CreateCommand())
                {
                    cmd.CommandText = "TK_EXTERNAL_EXECUTIONS_CTPY";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_orderId", OracleDbType.Int32).Value = orderId;
                    cmd.Parameters.Add("p_ctpyId", OracleDbType.Int32).Value = ctpy;
                    var retVal = cmd.Parameters.Add("p_retVal", OracleDbType.Int32, System.Data.ParameterDirection.Output);
                    cmd.ExecuteNonQuery();
                    CSMLog.Write("CSxFundSettleCtpyCheck", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, $"TK_EXTERNAL_EXECUTIONS_CTPY: {retVal.Value} executions updated");
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write("CSxFundSettleCtpyCheck", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, $"Exception Occured when executing TK_EXTERNAL_EXECUTIONS_CTPY: {ex.Message}");
            }
        }

        private List<string> getExternalSystemAllotmentCtpy()
        {
            string externalSystemAllotmentCtpy = "Manual,1820,10010910";
            try
            {
                sophis.misc.CSMConfigurationFile.getEntryValue("FundSettle", "ExternalSystemAllotmentCtpy", ref externalSystemAllotmentCtpy);
            }
            catch (Exception e)
            {
                CSMLog.Write("CSxFundSettleCtpyCheck", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, string.Format("Exception when getting ExternalSystemAllotmentCtpy: {0}", e.Message));
            }
            CSMLog.Write("CSxFundSettleCtpyCheck", MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug,
                string.Format("ExternalSystemAllotmentCtpy={0}", externalSystemAllotmentCtpy));
            return externalSystemAllotmentCtpy.Split(';').ToList();
        }
    }
}
