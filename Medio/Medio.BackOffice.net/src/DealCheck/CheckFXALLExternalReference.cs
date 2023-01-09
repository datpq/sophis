/*
** Includes
*/
using System;
using sophis;
using sophis.utils;
using sophis.portfolio;
using sophis.tools;
using sophis.oms;
using Sophis.DataAccess;
using System.Collections.Generic;
using System.Reflection;
using Oracle.DataAccess.Client;

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
    public class CheckFXALLExternalReference : sophis.backoffice_kernel.CSMCheckDeal
    {
        string className = "CheckFXALLExternalReference";

        public override void VoteForCreation(CSMTransaction transaction, int event_id)
        {

            try
            {
                long tradeId = transaction.getReservedCode();
                string fxallExternalReference = "";

                int orderID = transaction.GetOrderId(); //to check if same as fix message....gatlast placement ID from order Id..
                if (orderID > 0)
                {
                    int entity = transaction.GetEntity();

                    if (entity > 0)
                    {
                        fxallExternalReference = GetFXExternalReference(orderID, entity);

                        if (fxallExternalReference != "")
                        {
                            SetTradeFXALLExternalReference(tradeId, fxallExternalReference);
                            
                        }
                    }
                }
                else
                {
                    CSMLog.Write(className, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "No Valid OrderID from trade");
                }
            }
            catch (Exception ex)
            {
                CSMLog.Write(className, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Exception Caught : " + ex.Message);
            }

        }

        public string GetFXExternalReference(int OrderId, int Entity)
        {
            string retval = "";
            try
            {
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;
                    cmd.CommandText = "SELECT EXTERNAL_REFERENCE FROM MEDIO_FXALL_TEMP_EXTRNREF WHERE PLACEMENT_ID=(SELECT MAX(ID) FROM ORDER_PLACEMENT WHERE ORDERID=" + OrderId.ToString() + ") AND ENTITY=" + Entity.ToString();
                    CSMLog.Write(className, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug,"Executing Query : "+cmd.CommandText.ToString());
                    retval = cmd.ExecuteScalar() == DBNull.Value ? "" : cmd.ExecuteScalar().ToString();
                    retval = String.IsNullOrEmpty(retval) ? "" : retval;

                }
            }
            catch (Exception ex)
            { 
                CSMLog.Write(className, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error,"Exception Caught : "+ex.Message);
            }
            return retval;
        }

        public bool SetTradeFXALLExternalReference(long tradeId, string fxallExternalReference)
        {
            bool retval = false;
            try
            {

                CSMSwapTransactionReferencesCache extRefCache = CSMSwapTransactionReferencesCache.GetInstance();
                if (extRefCache != null)
                {
                    CSMSwapTransactionReferences refs = new CSMSwapTransactionReferences();
                    refs.SetRef(new CMString("FXALL_TRADE_ID"), new CMString(fxallExternalReference));
                    extRefCache.Save(tradeId, refs);
                }
                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;

                    //cleaning up MEDIO_FXALL_TEMP_EXTRNREF 
                    cmd.CommandText = "DELETE MEDIO_FXALL_TEMP_EXTRNREF WHERE EXTERNAL_REFERENCE='" + fxallExternalReference + "'";
                    CSMLog.Write(className, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Executing Query : " + cmd.CommandText.ToString());
                    cmd.ExecuteNonQuery();

                    // if no problems...
                    retval = true;

                }

            }
            catch (Exception ex)
            {
                CSMLog.Write(className, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Exception Caught : " + ex.Message);
            }

            return retval;

        }
    }
}