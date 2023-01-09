using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;
using System.Net.Mail;
using System.Reflection;
using System.IO;


using sophis.backoffice_kernel;
using sophis.portfolio;
using sophis.utils;
using sophis.tools;
using sophis.backoffice_otc;
using Sophis.DataAccess;
using sophis.oms.execution;
using sophisTools;
using Oracle.DataAccess.Client;

namespace MEDIO.BackOffice.net.src.KernelEngine
{
    public class CSxEventExecAveragePrice : sophis.tools.CSMAbstractEvent
    {

            private long fRefcon = 0;
            private int fEvtId = 0;

        private string fClassName = "CSxEventExecAveragePrice";

            public CSxEventExecAveragePrice(long refcon, int evtIdent)
                : base()
            {

                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Begin for transaction id: " + refcon + " evtId=" + evtIdent);
                fRefcon = refcon;
                fEvtId = evtIdent;
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "End.");
            }

            public override void Send()
            {
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Begin for transaction id: " + fRefcon + " evtId=" + fEvtId);

                // TODO : Get Average Price based on orderId, dateneg,dateval,counterparty..
                // waiting 1 second ...
               System.Threading.Thread.Sleep(500);

                CSMTransaction trans = CSMTransaction.newCSRTransaction(fRefcon);

                if (trans != null)
                {
                    int orderId = trans.GetOrderId();
                    int dateVal = trans.GetTransactionDate();
                    int dateNeg = trans.GetSettlementDate();
                    int counterparty = trans.GetCounterparty();

                    double averagePrice = GetAveragePrice(orderId, dateVal, dateNeg, counterparty);

                    if (Math.Abs(averagePrice) > 0)
                    {
                        // only if a price change we amend the trade (hence next time it comes here after DOaction, it won't loop.
                        double price = trans.GetSpot();
                    CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Current trade price " + price);

                    if (  Math.Abs(averagePrice - price) > Math.Pow(10,-8) ) 
                        {
                            trans.SetSpot(averagePrice);
                            price = trans.GetSpot();
                            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Spot modified " + price);
                        }
                    }
              
                	trans.RecomputeGrossAmount(true);
                    trans.DoAction(fEvtId);                   
                }
                else
                {
                    CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Issue retrieving transaction with Ident." + fRefcon);
                }
                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "End.");
            }

        public double GetAveragePrice(int orderId, int dateNeg, int dateVal, int ctpty)
        {

            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Entering  GetAveragePrice: orderid= "+ orderId+" dateNeg="+ dateNeg + " datVal=" + dateVal + " ctpty=" + ctpty);
            double retval = 0;

                try
                {
                    if (orderId > 0)
                    {

                        CSMDay dDateNeg = new CSMDay(dateNeg);
                        CSMDay dDateVal = new CSMDay(dateVal);

                        double averagePrice = 0.0;
                        double qty = 0;
                        IEnumerable<OrderExecution> executionList = null;

                        executionList = Sophis.OMS.Executions.OrderExecutionManager.Instance.GetExecutionsForOrders(new[] { orderId });

                        if (executionList != null)
                        {

                        foreach (OrderExecution exec in executionList)
                            {
                           
                            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Negociation date  has value = " + exec.NegotiationDate.HasValue);
                            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "ValueDate date has value = " + exec.ValueDate.HasValue);

                            int diffDateNeg = 0;
                            int diffDateVal = 0;
                            if (exec.NegotiationDate.HasValue)
                            {
                                DateTime execNegDate = (DateTime)exec.NegotiationDate;
                                 diffDateNeg = execNegDate.Year - dDateNeg.fYear + execNegDate.Month - dDateNeg.fMonth + execNegDate.Day - dDateNeg.fDay;
                            }

                            if (exec.ValueDate.HasValue)
                            {
                                DateTime execValDate = (DateTime)exec.ValueDate;
                                diffDateVal = execValDate.Year - dDateVal.fYear + execValDate.Month - dDateVal.fMonth + execValDate.Day - dDateVal.fDay;
                            }

                                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Using Exec Qty: " + exec.Quantity.ToString() + " , Price: " + exec.LastPrice.ToString() + " , Exec ID : " + exec.SophisExecID);
                                CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Checking Trade Date Neg = " + dDateNeg.ToString() + ", Date Val = " + dDateVal.ToString() + " , CTPY = " + ctpty);
                                //CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Checking Exec Date Neg = " + execNegDate + " , Date Val = " + execValDate + " , CTPY = " + exec.CounterpartyID);

                                //TODO Check for dateval, dateng and ctpty..
                                //trade.SetTransactionDate((m_DataModel.NegotiationDate - new DateTime(1904,1,1)).Days);
                                if ((diffDateNeg == 0) && (diffDateVal == 0) && (ctpty == exec.CounterpartyID))
                                {
                                    averagePrice += ((double)exec.Quantity * (double)exec.LastPrice);
                                    qty += (double)exec.Quantity;
                                }

                            }

                            if (Math.Abs(qty) > 0)
                                averagePrice = averagePrice / qty;

                            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Computed Average Price : " + averagePrice);

                            retval = averagePrice;

                            //using (var cmd = new OracleCommand())
                            //{
                            //    cmd.Connection = DBContext.Connection;

                            //    cmd.CommandText = "select  CAST((SUM(Q * P) / SUM(Q)) AS DECIMAL(10,8)) From (Select distinct E.PK_SOPHISEXECID, E.LASTQUANTITY Q, E.LASTPRICE P from HISTOMVTS H, EXTERNAL_EXECUTIONS_TO_TRADES L, EXTERNAL_EXECUTIONS E where H.SOPHIS_ORDER_ID = " + orderId + " and H.DATENEG = num_to_date(" + dateNeg + ") and H.DATEVAL = num_to_date(" + dateVal + ") and H.CONTREPARTIE = " + ctpty + " and   L.TRADEID = H.REFCON and   E.PK_SOPHISEXECID = L.SOPHISEXECID)";
                            //    CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Executing Query: " + cmd.CommandText.ToString());
                            //    using (OracleDataReader reader = cmd.ExecuteReader())
                            //    {
                            //        while (reader.Read())
                            //        {
                            //            retval = Convert.ToDouble(reader[0]);
                            //            CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_info, "Retrieved " + reader[0].ToString());

                            //        }
                            //    }
                            //}

                        }
                    }
                }
                catch (Exception ex)
                {
                    CSMLog.Write(fClassName, MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Exception Caught : " + ex.Message);
                }

                return retval;
            }



    }
}
