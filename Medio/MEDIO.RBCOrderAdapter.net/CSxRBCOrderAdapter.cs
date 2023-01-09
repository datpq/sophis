using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using sophis.oms;
using sophis.orderadapter;
using sophis.utils;
using sophis.log;
using sophis.wcfFramework.config;
using Oracle.DataAccess.Client;
using sophis.backoffice_kernel;
using Sophis.OMS.Market;
using Sophis.OrderBookCompliance;

namespace MEDIO.RBCOrderAdapter
{
    public class CSxRBCOrderAdapter : sophis.orderadapter.IAdapter
    {
        #region Fields
        private OrderAdapterDescriptor fDescriptor = new OrderAdapterDescriptor();
        CSxFTPManager ftpInstance = new CSxFTPManager();
        private IOrderManagerForOA managerOA = null;
        private IOrderManagerService manager = null;
        private CSxFileHandler fFileHandler = new CSxFileHandler(RBCConfigurationSectionGroup.RBCFileSection.ToRBCFolder, RBCConfigurationSectionGroup.RBCFileSection.OutFileExtension, RBCConfigurationSectionGroup.RBCFileSection.FileSeparator);
        private int fUserId = 1;
        private int fUserProfileId = 0;
        public static CSMApi api = new CSMApi();
        private CSxFileWatcher fFileWatcher = null;
        public static string cptyForRBC = "";


        #endregion

        #region OA Methods
        public OrderAdapterDescriptor GetOrderAdapterDescriptor()
        {
            return fDescriptor;
        }
        /// <summary>
        /// Initialization.
        /// </summary>
        public void init()
        {
            //Do initializations here. This is called after the sophis framework is initialized
            using (Logger log = new Logger(this, "init"))
            {
                log.log(Severity.debug, "Start");

                fDescriptor.AdapterName = ConfigUtil.getServerInstance();
                log.log(Severity.info, "AdapterName = '" + fDescriptor.AdapterName + "'");

                fDescriptor.InstrumentTypes = new int[] { ESecurityType.Bond, ESecurityType.Equity, ESecurityType.Unspecified, ESecurityType.InternalFund };
                // Implemented adapter features (see sophis.orderadapter.OrderAdapterFeature)
                fDescriptor.Features = new int[] { OrderAdapterFeature.SingleOrder, OrderAdapterFeature.MultiLegsOrder, OrderAdapterFeature.GroupOrder };
                // Supported instrument universal references 
                fDescriptor.IntrumentReferences = new string[0];
                // Supported third party references/properties 
                fDescriptor.ThirdPartyReferences = new string[] { "REFERENCE" };

                try
                {
                    if (api != null)
                    {
                        api.Initialise();
                        api.ActivateRT();
                    }
                }
                catch (Exception ex)
                {
                    log.log(Severity.error, string.Format("Cannot initialise API: [{0}]", ex.Message));
                    log.end();
                    throw;
                }

                log.end();
            }
        }

        /// <summary>
        /// Start.
        /// </summary>
        public void start()
        {
            //Called after the init. Starting from now on I can receive OMS requests

            using (Logger log = new Logger(this, "start"))
            {
                // Access to Order Book
                log.log(Severity.debug, string.Format("Setting Context to User {0} and Profile {1}", fUserId, fUserProfileId));
                OrderManagerConnector.Instance.SetClientContext(fUserId, fUserProfileId, "");

                for (int i = 0; i < 5 && managerOA == null; ++i)
                {
                    Thread.Sleep(5000);
                    managerOA = OrderManagerConnector.Instance.GetOrderManagerForOA();
                }
                if (managerOA == null)
                {
                    log.log(Severity.error, "Unable to Reach Order Manager for Order Adapter");
                    log.end();
                    throw new Exception("Unable to Reach Order Manager for Order Adapter");
                }

                manager = OrderManagerConnector.Instance.GetOrderManager();
                if (manager == null)
                {
                    log.log(Severity.error, "Unable to Reach Order Manager");
                    log.end();
                    throw new Exception("Unable to Reach Order Manager");
                }
                log.log(Severity.debug, "End");

                try
                {
                    fFileWatcher = new CSxFileWatcher(RBCConfigurationSectionGroup.RBCFileSection.FromRBCFolder, manager, 10000);
                    fFileWatcher.Start(10000);
                }

                catch (Exception ex)
                {
                    log.log(Severity.error, string.Format("Cannot Instantiate File Manager: [{0}]", ex.Message));
                    log.end();
                    throw;
                }


                string query = "select counterparty from order_defparam_selector where external_system='"+RBCConfigurationSectionGroup.RBCFileSection.OrderAdapterName+"'";
                int counterpartyId = 0;
                string cptyForRBCId = "";
                try
                {
                    using (OracleCommand command = new OracleCommand())
                    {
                        command.Connection = Sophis.DataAccess.DBContext.Connection;
                        command.CommandText = query;
                        object scalar = command.ExecuteScalar();

                        if (scalar != null)
                            cptyForRBCId = Convert.ToString(scalar);

                        if (Int32.TryParse(cptyForRBCId, out counterpartyId))
                        {
                            using (CSMThirdParty orderCpty = CSMThirdParty.GetCSRThirdParty(counterpartyId))
                            {
                                using (CMString cptyRef = "")
                                {
                                    orderCpty.GetReference(cptyRef);
                                    cptyForRBC = cptyRef.ToString();
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    log.log(sophis.log.Severity.warning, " Exception: " + ex);
                }

                log.end();
            }
        }

        /// <summary>
        /// Stop.
        /// </summary>
        public void stop()
        {
            //Called when the process is closed (on CTRL+C), before stopping the Sophis framework
        }

        /// <summary>
        /// Order creation.
        /// </summary>

        public void createOrder(IOrder order)
        {
            using (Logger log = new Logger(this, "createOrder"))
            {
                log.log(Severity.debug, "Start");
                log.log(Severity.info, string.Format("Handling Order {0}", order.ID));

                // using separate thread
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        if (order is OrderGroup)
                        {
                            /*
                            List<OAMessage> grpMsg = new List<OAMessage>();
                            grpMsg.Add(new OAOrderNewMessage(-1L, order.ID, order.ID.ToString()));
                            managerOA.OnOAOrderMessage(grpMsg);
                            */

                        }

                        // msg list to Order Book
                        List<IOrder> orders = new List<IOrder>();
                        if (order is SingleOrder || order is MultiLegsOrder)
                        {
                            orders.Add(order);
                        }
                        else
                        {
                            foreach (IOrder inner in order.GetInnerOrders())
                            {
                                orders.Add(inner);
                            }
                        }
                        List<OAMessage> msgs = new List<OAMessage>();
                        if (orders.Count > 0)
                        {
                            log.log(Severity.info, string.Format("Number of InnerOrders for Order {0} to Create: {1}", order.ID, orders.Count));
                        }
                        foreach (IOrder inner in orders)
                        {
                            msgs.Add(createOrderToFile(inner));
                        }

                        // managerOA.OnOAOrderMessage(msgs);
                    }
                    catch (Exception ex)
                    {
                        List<OAMessage> errs = new List<OAMessage>();
                        OAMessage err = new OAOrderRejectedMessage(-1L, order.ID, -1, ex.Message);
                        errs.Add(err);
                        managerOA.OnOAOrderMessage(errs);
                        log.log(Severity.error, string.Format("Unexpected Error: [{0}]", ex));
                    }
                });

                log.log(Severity.debug, "End");
                log.end();
            }
        }

        public void createOrders(IOrder[] orders)
        {
            using (Logger log = new Logger(this, "createOrders"))
            {
                log.log(Severity.debug, "Start");
                log.log(Severity.info, string.Format("Number of Orders to Create: {0}", orders.Length));
                foreach (IOrder order in orders)
                {
                    createOrder(order);
                }
                log.log(Severity.debug, "End");
            }
        }

        /// <summary>
        /// Order cancellation.
        /// </summary>
        public void cancelOrder(IOrder order)
        {
            using (Logger log = new Logger(this, "cancelOrder"))
            {
                log.log(Severity.debug, "Start");

                // using separate thread
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        // msg list to Order Book
                        List<OAMessage> msgs = new List<OAMessage>();
                        if (!(order is IGroup))
                        {
                            msgs.Add(new OAOrderCancelledWithReasonMessage(-1L, order.ID, order.ID.ToString()));
                        }
                        else
                        {
                            // Send new for all inner components
                            IGroup group = (IGroup)order;
                            foreach (IOrder inner in group.GetUnderlyingOrders(false))
                            {
                                msgs.Add(new OAOrderCancelledWithReasonMessage(-1L, inner.ID, inner.ID.ToString()));
                            }
                        }
                        managerOA.OnOAOrderMessage(msgs);
                    }
                    catch (Exception ex)
                    {
                        OAMessage err = new OAOrderCancelRejectedMessage(-1L, order.ID, -1, ex.Message);
                        managerOA.OnOAOrderMessage((IEnumerable<OAMessage>)err);
                        log.log(Severity.error, string.Format("Unexpected Error: [{0}]", ex));
                    }
                });

                log.log(Severity.debug, "End");
                log.end();
            }

        }

        public void cancelOrders(IOrder[] orders)
        {
            foreach (IOrder order in orders)
            {
                cancelOrder(order);
            }
        }


        public void replaceOrder(int parent, IOrder order)
        {
            using (Logger log = new Logger(this, "replaceOrder"))
            {
                log.log(Severity.debug, "Start");

                // using separate thread
                ThreadPool.QueueUserWorkItem(delegate
                {
                    try
                    {
                        // msg list to Order Book
                        List<OAMessage> msgs = new List<OAMessage>();
                        if (!(order is IGroup))
                        {
                            msgs.Add(new OAOrderReplacedMessage(-1L, order.ID, order.ID, order.ID.ToString()));
                        }
                        else
                        {
                            // Send new for all inner components
                            IGroup group = (IGroup)order;
                            foreach (IOrder inner in group.GetUnderlyingOrders(false))
                            {
                                msgs.Add(new OAOrderReplacedMessage(-1L, inner.ID, inner.ID, inner.ID.ToString()));
                            }
                        }
                        managerOA.OnOAOrderMessage(msgs);
                    }
                    catch (Exception ex)
                    {
                        OAMessage err = new OAOrderReplaceRejectedMessage(-1L, order.ID, order.ID, -1, ex.Message);
                        managerOA.OnOAOrderMessage((IEnumerable<OAMessage>)err);
                        log.log(Severity.error, string.Format("Unexpected Error: [{0}]", ex));
                    }
                });

                log.log(Severity.debug, "End");
                log.end();
            }
                 
           
        }
        #endregion

        #region OrderUtils

        private OAMessage createOrderToFile(IOrder order)
        {
            using (Logger log = new Logger(this, "createOrderToFile"))
            {
                log.log(Severity.debug, "Start");
                OAMessage result = null;
                int orderId = OrderManagerConnector.Instance.GetOrderManager().GetOrderById(order.ID).ID;
                string errMessage, fileName;
                log.log(Severity.info, string.Format("Handling Order {0}", orderId));
                if (createExternalFundOrder(order, out errMessage, out fileName))
                {
                    result = new OAOrderNewMessage(-1L, order.ID, order.ID.ToString());
                    log.log(Severity.info
                        , string.Format("SUCCESS; Order - ID = {0}; Placement = {1} [{2}]", orderId, result.PlacementId, result.ResponseMessage));

                    //upload the request file to the RBC FTP                          
                    //bool uploadDone = ftpInstance.UploadToFTP(fileName);

                    //string currentFile = string.Format("{0}\\{1}.{2}", RBCConfigurationSectionGroup.RBCFileSection.ToRBCFolder, fileName, RBCConfigurationSectionGroup.RBCFileSection.OutFileExtension);
                    //string fileSent = string.Format("{0}\\{1}{2}.{3}", RBCConfigurationSectionGroup.RBCFileSection.ToRBCFolder, "[SENT]", fileName, RBCConfigurationSectionGroup.RBCFileSection.OutFileExtension);
                    //string fileNotSent = string.Format("{0}\\{1}{2}.{3}", RBCConfigurationSectionGroup.RBCFileSection.ToRBCFolder, "[NOT SENT]", fileName, RBCConfigurationSectionGroup.RBCFileSection.OutFileExtension);

                    //if (uploadDone == true)
                    //{
                    //    if (File.Exists(currentFile))
                    //    {
                    //        File.Move(currentFile, fileSent);
                    //        File.Delete(currentFile);
                    //    }
                    //}
                    //else
                    //{
                    //    if (File.Exists(currentFile))
                    //    {
                    //        File.Move(currentFile, fileNotSent);
                    //        File.Delete(currentFile);
                    //    }
                    //}
                }
                else
                {
                    result = new OAOrderRejectedMessage(-1L, order.ID, -1, errMessage);
                    log.log(Severity.error
                        , string.Format("FAILURE; Order - ID = {0}; Placement = {1} [{2}]", orderId, result.PlacementId, result.ResponseMessage));
                }

                log.log(Severity.debug, "End");
                return result;
            }
        }


        private bool createExternalFundOrder(IOrder order, out string errMessage, out string fileName)
        {
            using (Logger log = new Logger(this, "createExternalFundOrder"))
            {
                log.log(Severity.debug, "Start");
                bool result = true;
                bool isFieldsOK = true;
                errMessage = "";
                fileName = "";

                // init record if needed
                List<CSxFileHandler.Column> record = new List<CSxFileHandler.Column>();
                const int nbColumns = 18;
                for (int i = 0; i < nbColumns; ++i)
                {
                    record.Add(new CSxFileHandler.Column(1, CSxFileHandler.eColumnType.eString, .0, 0, "", 0));
                }

                try
                {
                    int Step = 1;
                    int orderId = manager.GetOrderById(order.ID).ID;
                    log.log(Severity.info, string.Format("Start Handling Order {0}...", orderId));
                    log.log(Severity.info, string.Format("Step {0} : Checking OrderTargetOwners...", Step++));

                    // target
                    IOrderTargetOwner target = null;
                    if (order.GetOrderTargetOwners() != null && order.GetOrderTargetOwners().Count == 1)
                    {
                        target = order.GetOrderTargetOwners().First();
                    }
                    else
                    {
                        log.log(Severity.error, string.Format("Order {0} No Target or Inconsistent Target Found", orderId));
                        isFieldsOK = false;
                    }

                    // set fields

                    //account-no
                    // SELECT account_at_custodian FROM bo_treasury_account WHERE id in (SELECT acc_id FROM bo_treasury_ext_ref WHERE ref_id = 3 AND value = '<target_folio>');

                    SingleOrder singleOrder = (SingleOrder)order;

                    AllocationRulesSet allocations = singleOrder.AllocationRulesSet;
                    int nbAllocation = 0;
                    //AllocationRule allocationRule = singleOrder.AllocationRulesSet.Allocations.ElementAt(nbAllocation);
                    if (allocations != null)
                    {
                        log.log(Severity.info, string.Format("Step {0} : Checking AllocationRulesSet...", Step++));
                        int count = allocations.Allocations.Count;
                        if (nbAllocation <= count && count >= 1)
                        {
                            AllocationRule allocationRule = singleOrder.AllocationRulesSet.Allocations.ElementAt(nbAllocation);
                            if (allocationRule != null)
                            {
                                int portfId = allocationRule.PortfolioID;
                                string accountAtCustodian = "";
                                string query = " SELECT account_at_custodian FROM bo_treasury_account WHERE id in (SELECT acc_id FROM bo_treasury_ext_ref WHERE ref_id = 3 AND value = '" + portfId + "') and account_at_custodian NOT LIKE '%GS'";

                                log.log(Severity.info, string.Format("Step {0} : Checking AccountAtCustodian (account-no)...", Step++));
                                try
                                {
                                    using (OracleCommand command = new OracleCommand())
                                    {
                                        command.Connection = Sophis.DataAccess.DBContext.Connection;
                                        command.CommandText = query;
                                        object scalar = command.ExecuteScalar();
                                        if (scalar != null)
                                            accountAtCustodian = Convert.ToString(scalar);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.log(sophis.log.Severity.warning, " Exception: " + ex);
                                }
                                if (accountAtCustodian != "")
                                {
                                    record[0].SetString(25, accountAtCustodian);
                                }
                                else 
                                {
                                    log.log(Severity.error, string.Format("Order {0}: Account number not found for portfolio {1}", orderId, portfId));
                                    isFieldsOK = false;
                                }
                            }
                        }
                    }
                    //h-status
                    record[1].SetString(1, "P");

                    //order date
                    record[3].SetDate(order.CreationInfo.TimeStamp);

                    //tran-code
                    log.log(Severity.info, string.Format("Step {0} : Checking Order Side (tran-code)...", Step++));
                    string side = "";
                    switch (order.Side)
                    {
                        case ESide.Buy:
                            side = "CTB";
                            break;
                        case ESide.Sell:
                            side = "BTC";
                            break;
                        default:
                            log.log(Severity.error, string.Format("Order {0}: Not a Buy or a Sell", orderId));
                            isFieldsOK = false;
                            break;
                    }
                    log.log(Severity.debug, string.Format("Order {0} Set Field 5 = [{1}]", orderId, side));
                    record[4].SetString(10, side);

                    //local-currency
                    log.log(Severity.info, string.Format("Step {0} : Checking Currency (local-currency)...", Step++));
                    var ccy = ""; 
                    using (OracleCommand command = new OracleCommand())
                    {
                        string query = String.Format("select DEVISE_TO_STR({0}) from dual", target.Target.Currency);
                        command.Connection = Sophis.DataAccess.DBContext.Connection;
                        command.CommandText = query;
                        ccy = command.ExecuteScalar().ToString();
                        record[12].SetString(3, ccy.ToUpper());
                    }

                    //all-shares flag : available on order property
                    //record[14]
                    log.log(Severity.info, string.Format("Step {0} : Checking FullRedemption property (all-shares flag)...", Step++));
                    object allShares = singleOrder.GetOrderPropertyValue("RBC", "FullRedemption");
                    string fullRedemption = "";
                    if (allShares != null)
                    {
                        fullRedemption = allShares.ToString();
                        if (fullRedemption != "")
                            record[14].SetString(3, fullRedemption);
                        else
                        {
                            isFieldsOK = false;
                            log.log(Severity.error, string.Format("Order {0}: FullRedemption order property value is empty", orderId));
                        }
                    }
                    else
                    {
                        log.log(Severity.error, string.Format("Order {0}: FullRedemption order property value is not available", orderId));
                        isFieldsOK = false;
                    }

                    //trn-id
                    record[15].SetInteger(10, orderId);
                    log.log(Severity.info, string.Format("Step {0} : Checking TradingUnit, ISIN (fund-id-ext), Ticker (bloomberg-sec-code) properties...", Step++));
                    int instrumentCode = target.Target.SecurityID;

                    var sql = "SELECT T.QUOTITE, T.REFERENCE, E.VALUE FROM TITRES T, EXTRNL_REFERENCES_INSTRUMENTS E WHERE T.SICOVAM = E.SOPHIS_IDENT AND E.REF_IDENT = 1 AND T.SICOVAM = " + instrumentCode;
                    var tradingUnit = 1.0;
                    var ISINStr = "";
                    string tickerStr = "";
                    try
                    {
                        using (OracleCommand command = new OracleCommand())
                        {
                            command.Connection = Sophis.DataAccess.DBContext.Connection;
                            command.CommandText = sql;

                            using (OracleDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    tradingUnit = reader[0] != DBNull.Value ? (double)Convert.ChangeType(reader[0], typeof(double)) : default(double);
                                    tickerStr = reader[1] != DBNull.Value ? (string)Convert.ChangeType(reader[1], typeof(string)) : default(string);
                                    ISINStr = reader[2] != DBNull.Value ? (string)Convert.ChangeType(reader[2], typeof(string)) : default(string);
                                }
                            }
                        }
                    }
                    catch (Exception e )
                    {
                        log.log(Severity.error, e.Message);
                    }

                    if (fullRedemption == "Yes")
                    {
                        record[5].SetString(10,"");
                        record[6].SetString(10,"");
                    }
                    else
                    {
                        EQuantityType amountType = order.QuantityData.OrderedType;
                        if (amountType == EQuantityType.Amount) //order-amt
                        {
                            double orderQty = order.QuantityData.OrderedQty;
                            if (!ccy.Equals(ccy.ToUpper())) // GBp
                                orderQty = order.QuantityData.OrderedQty / 100;
                            record[5].SetDouble(10, orderQty, 2);
                        }
                        else if (amountType == EQuantityType.Share) //order-shares
                        {
                            int digits = GetDigitsCount(tradingUnit);
                            record[6].SetDouble(10, order.QuantityData.OrderedQty, digits);
                        }
                        else if (amountType == EQuantityType.Unit) //order-Unit
                        {
                            int digits = GetDigitsCount(tradingUnit);
                            record[6].SetDouble(10, order.QuantityData.OrderedQty * tradingUnit, digits);
                        }
                    }
                    //fund-id-ext
                    //record[16]
                    if (ISINStr != "")
                        record[16].SetString(15, "ISIN:" + ISINStr);
                    else
                    {
                        log.log(Severity.error, string.Format("Order {0}: ISIN reference is not available", orderId));
                        isFieldsOK = false;
                    }
                    //bloomberg-sec-code
                    //record[17]
                    if (tickerStr != "")
                        record[17].SetString(40, tickerStr);
                    else
                    {
                        log.log(Severity.error, string.Format("Order {0}: Ticker reference is not available", orderId));
                        isFieldsOK = false;
                    }

                    // write to file
                    log.log(Severity.info, string.Format("Step {0} : Start writing order to file...", Step++));
                    result = fFileHandler.WriteToFile(orderId, record, out fileName);
                    log.log(Severity.info, string.Format("Step {0} : End writing order to file {1} [Status:{2}].", Step, fileName, result));
                    
                    if (!isFieldsOK)
                    {
                        errMessage = string.Format("Order {0} - Some Fields are Incorrect.Please check!", orderId);
                        log.log(Severity.error, string.Format("Order {0} - Some Fields are Incorrect", orderId));
                    }

                    log.log(Severity.info, string.Format("End Handling Order {0}...", orderId));
                    log.end();
                }
                catch (Exception ex)
                {
                    result = false;
                    log.log(Severity.error, string.Format("Unexpected Error: [{0}]", ex));
                    log.end();
                }
                return result;
            }
        }

        private static int GetDigitsCount(double value)
        {
            return (int)Math.Round(Math.Abs(Math.Log(value) / Math.Log(10)), 0);
        }

        #endregion
    }
}
