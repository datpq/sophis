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
using sophis.oms.execution;
using sophisTools;

namespace MEDIO.BackOffice.net.src.DealCondition
{
    class CheckExecutionAveragePrice : sophis.backoffice_kernel.CSMCheckDeal
    {

        string classname = "CheckExecutionAveragePrice";

        public override void VoteForCreation(CSMTransaction transaction, int event_id)
        {
               
                int orderId = transaction.GetOrderId();

                if (orderId > 0)
                {                 
                    int ctpty = transaction.GetCounterparty();
                    int dateNeg = transaction.GetTransactionDate();
                    int dateVal = transaction.GetSettlementDate();

                    double avgPrice = GetAveragePrice(orderId,dateNeg,dateVal,ctpty);

                    transaction.SetSpot(avgPrice);
                }
        }

        public override void VoteForModification(CSMTransaction original, CSMTransaction transaction, int event_id)
        {

            IEnumerable<OrderExecution> executionList = null;

            int orderId = original.GetOrderId();
            int execVenue = transaction.GetExecutionVenue();
            double oQty = original.GetQuantity();
            double oSpot = original.GetSpot();

            double tQty = transaction.GetQuantity();
            double tSpot = transaction.GetSpot();

            CSMLog.Write(classname, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "In VoteForModification with Org Qty = " + oQty.ToString()+" OrgSpot = "+oSpot.ToString()+" tQty = "+tQty.ToString()+" tSpot = "+tSpot.ToString());
            CSMLog.Write(classname, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Codes: " + original.getReservedCode().ToString() + "," + original.GetTransactionCode().ToString() + "," + transaction.getReservedCode().ToString() + "" + transaction.GetTransactionCode());
            double averagePrice = 0.0;
            double qty = 0;

            if (orderId > 0)
            {
                executionList = Sophis.OMS.Executions.OrderExecutionManager.Instance.GetExecutionsForOrders(new[] { orderId });

                if (executionList != null)
                {

                    foreach (OrderExecution exec in executionList)
                    {

                        int execId = exec.SophisExecID;
                        CSMLog.Write(classname, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Using Exec Qty: " + exec.Quantity.ToString() + " , Price: " + exec.LastPrice.ToString());
                        averagePrice += ((double)exec.Quantity * (double)exec.LastPrice);
                        qty += (double)exec.Quantity;

                        OrderPlacement oplacement = exec.Placement;
                        
                    }

                    if (qty > 0)
                        averagePrice = averagePrice / qty;

                    CSMLog.Write(classname, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Computed Average Price : " + averagePrice);

                }

                int ctpty = transaction.GetCounterparty();
                int dateNeg = transaction.GetTransactionDate();
                int dateVal = transaction.GetSettlementDate();



              // TODO : Get the proper placement ID from parameter below....

                using (var cmd = new OracleCommand())
                {
                    cmd.Connection = DBContext.Connection;

                    cmd.CommandText = "select PK_TICKETID,SOPHISEXECID,LASTPRICE,LASTQUANTITY from EXTERNAL_TICKETS where sophisorderid=17871";// +orderId.ToString();


                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                           
                            CSMLog.Write(classname, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Retrieved PK_TICKETID : " + reader[0].ToString()+" , EXECID : "+reader[1].ToString()+" , LASTPRICE : "+reader[2].ToString()+" , LASTQUANTIY : "+ reader[3].ToString());

                        }
                    }
                }


                transaction.SetSpot(averagePrice);
            }
        }


        // TODO TO MODIFY TO USE QUERY ABOVE TO COMPUTE AVG PRICE
        public double GetAveragePrice(int orderId, int dateNeg, int dateVal, int ctpty)
        {
            double retval = 0.0;
            try
            {

                    using (var cmd = new OracleCommand())
                    {
                    cmd.Connection = DBContext.Connection;

                    cmd.CommandText = "select  CAST((SUM(Q * P) / SUM(Q)) AS DECIMAL(10,8)) From (Select distinct E.PK_SOPHISEXECID, E.LASTQUANTITY Q, E.LASTPRICE P from HISTOMVTS H, EXTERNAL_EXECUTIONS_TO_TRADES L, EXTERNAL_EXECUTIONS E where H.SOPHIS_ORDER_ID = "+orderId+" and H.DATENEG = num_to_date("+dateNeg+") and H.DATEVAL = num_to_date("+dateVal+") and H.CONTREPARTIE = "+ctpty+" and   L.TRADEID = H.REFCON and   E.PK_SOPHISEXECID = L.SOPHISEXECID)";
                    CSMLog.Write(classname, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Executing Query: " + cmd.CommandText.ToString());
                        using (OracleDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                retval = Convert.ToDouble(reader[0]);
                                CSMLog.Write(classname, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Retrieved " + reader[0].ToString());

                            }
                        }
                    }
            }
            catch (Exception ex)
            {
                CSMLog.Write(classname, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Exception Caught : " + ex.Message);
            }
            return retval;
        }


    }
  
}
