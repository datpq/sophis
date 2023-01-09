using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;
using sophis.oms;
using sophis.orderadapter;
using Sophis.WF.Core;
using sophis.oms.execution;
using Sophis.OMS.Activities;
using sophis.log;
using Oracle.DataAccess.Client;
using sophis.portfolio;
using sophis.backoffice_kernel;
using Sophis.OMS;
using Sophis.OMS.Market;
using Sophis.OrderBookCompliance;
using sophis.misc;


namespace MEDIO.RBCOrderAdapter
{
    public sealed class CSxFileWatcher : FileSystemWatcher
    {
        #region fields
        private Thread fTread;
        private readonly object mutex = new object();
        private const int fMaxWait = 5000;
        private int fPollPeriod = 1000;
        public IOrderManagerService fOrderManager;
        private List<string> fOverallocStatuses = new List<string>();
        #endregion

        #region public

        public CSxFileWatcher(string path, IOrderManagerService OrderManager, int pollPeriod)
        {
            using (Logger log = new Logger(this, "FileProcessing"))
            {
                log.log(Severity.debug, "Start");

                try
                {
                    // FileSystemWatcher
                    if (!String.IsNullOrEmpty(path))
                    {
                        Path = path;
                    }
                    else
                    {
                        const string errMessage = "Path is Empty or Null";
                        log.log(Severity.error, errMessage);
                        log.end();
                        throw new Exception(errMessage);
                    }
                    LoadOverallocStatusesFromConfig();
                    Filter = "";//watch all file types

                    NotifyFilter = NotifyFilters.LastWrite;
                    IncludeSubdirectories = false;

                    // specific
                    Changed += onFileSystemChanged;
                    fOrderManager = OrderManager;
                    fPollPeriod = pollPeriod;
                }
                catch (Exception e)
                {
                    log.log(Severity.error, e.Message);
                    log.end();
                    throw;
                }

                log.log(Severity.debug, "End");
                log.end();
            }
        }

        public CSxFileWatcher()
        {
            throw new NotImplementedException();
        }

        public CSxFileWatcher(string path)
        {
            throw new NotImplementedException();
        }

        public CSxFileWatcher(string path, string filter)
        {
            throw new NotImplementedException();
        }

        public void Start(int period)
        {
            using (Logger log = new Logger(this, "Start"))
            {
                log.log(Severity.debug, "Start");

                // poll period
                if (period != 0)
                {
                    fPollPeriod = period;
                }

                // worker
                fTread = new Thread(run) { Name = GetType().Name };
                fTread.Start();

                // main
                EnableRaisingEvents = true;

                log.log(Severity.debug, "End");
                log.end();
            }
        }

        public void Terminate()
        {
            using (Logger log = new Logger(this, "Dispose"))
            {
                log.log(Severity.debug, "Start");

                EnableRaisingEvents = false;
                if (fTread != null)
                {
                    fTread.Interrupt();
                    fTread.Join(fMaxWait);
                }

                log.log(Severity.debug, "End");
                log.end();
            }
        }
        #endregion



        #region private
        private void onFileSystemChanged(object source, FileSystemEventArgs e)
        {
            using (Logger log = new Logger(this, "onFileSystemChanged"))
            {
                log.log(Severity.debug, "Start");

                lock (mutex)
                {
                    log.log(Severity.debug, "Pulse");
                    Monitor.PulseAll(mutex);
                }

                log.log(Severity.debug, "End");
                log.end();
            }
        }
        #endregion

        #region private
        private void run()
        {
            {
                using (Logger log = new Logger(this, "run"))
                {
                    log.log(Severity.debug, "Start");

                    while (true)
                    {
                        lock (mutex)
                        {
                            try
                            {
                                getFiles();
                                Monitor.Wait(mutex, fPollPeriod);
                            }
                            catch (ThreadInterruptedException)
                            {
                                log.log(Severity.debug, "File Processing Interrupted");
                                log.end();
                            }
                            catch (Exception e)
                            {
                                string errMessage = string.Format("Unexpected Exception: [{0}]", e.Message);
                                log.log(Severity.error, errMessage);
                                log.end();
                                throw;
                            }
                        }
                    }
                }
            }
        }

        private void getFiles()
        {
            using (Logger log = new Logger(this, "getFiles"))
            {
                log.log(Severity.debug, "Start");

                try
                {
                    EnableRaisingEvents = false;

                    List<string> files = Directory.GetFiles(Path, Filter, SearchOption.TopDirectoryOnly).ToList();
                    List<string> sortedFiles = files.OrderBy(f => Directory.GetLastWriteTimeUtc(f)).ToList();
                    if (sortedFiles.Count != 0)
                    {
                        processFiles(sortedFiles.ToArray());
                    }

                    EnableRaisingEvents = true;
                }
                catch (Exception e)
                {
                    log.log(Severity.error, string.Format("Unexpected Error: [{0}]", e.Message));
                    log.end();
                    throw;
                }

                log.log(Severity.debug, "End");
                log.end();
            }
        }


        private void processFiles(string[] filesFullPath)
        {
            using (Logger log = new Logger(this, MethodBase.GetCurrentMethod().Name))
            {
                log.log(Severity.debug, "Start");

                try
                {
                    foreach (string file in filesFullPath)
                    {
                        string fileExtension = System.IO.Path.GetExtension(file);
                        string eventName = "";

                        if (fileExtension == string.Format(".{0}", RBCConfigurationSectionGroup.RBCFileSection.EXECFileExtension).ToLower()
                          || fileExtension == string.Format(".{0}", RBCConfigurationSectionGroup.RBCFileSection.EXECFileExtension).ToUpper())
                        {
                            eventName = RBCConfigurationSectionGroup.WfEventsSection.ExecutionEvent;
                            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                            {
                                using (StreamReader reader = new StreamReader(stream))
                                {
                                    bool headerLine = true;
                                    string rmaSophisFile = string.Format("{0}\\{1}.{2}", RBCConfigurationSectionGroup.RBCFileSection.RMAdapterFilePath, RBCConfigurationSectionGroup.RBCFileSection.RMAdapterFileName, RBCConfigurationSectionGroup.RBCFileSection.RMAdapterFileExt);
                                    string line;
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (headerLine)
                                        {
                                            headerLine = false;
                                        }
                                        else
                                        {

                                            log.log(Severity.debug, string.Format("Read Line [{0}] in Execution File [{1}]", line, file));
                                            string[] split = line.Split(RBCConfigurationSectionGroup.RBCFileSection.FileSeparator);

                                            if (split.Length < 18)
                                            {
                                                log.log(Severity.error, string.Format("Line [{0}] does not have corresponding format", line));
                                            }
                                            else
                                            {
                                                int orderId = 0;
                                                if (!Int32.TryParse(split[15], out orderId))
                                                    log.log(Severity.error, string.Format("Line [{0}]: Order id not available", line));

                                                if (orderId != 0)
                                                {
                                                    IOrder order = fOrderManager.GetOrderById(orderId);
                                                    if (order != null)
                                                    {
                                                        SingleOrder singleOrder = (SingleOrder)order;
                                                        bool sanityCheck = ExecutionSanityCheck(split, singleOrder);

                                                        // check ccy 
                                                        var ccy = GetCcy(singleOrder.Target.Currency);

                                                        IList<OrderPlacement> placementList = fOrderManager.GetOrderPlacements(orderId);
                                                        var placementId = "";
                                                        if (placementList.LastOrDefault() != null)
                                                            placementId = placementList.LastOrDefault().Id.ToString();
                                                        else
                                                            log.log(Severity.error, string.Format("Cannot find a placement for order {0}", orderId));
                                                        
                                                        char buyOrSale = '-';
                                                        if (split[4] == "CTB") buyOrSale = 'B';
                                                        else if (split[4] == "BTC") buyOrSale = 'S';

                                                        string timeResult = "";
                                                        DateTime now = DateTime.Now;
                                                        timeResult = now.ToString("HHmmss");

                                                        DateTime setlDate = DateTime.ParseExact(split[9], "dd/MM/yyyy", CultureInfo.CurrentCulture);
                                                        string setlDateFormat = setlDate.Date.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
                                                        DateTime negDate = DateTime.ParseExact(split[3], "dd/MM/yyyy", CultureInfo.CurrentCulture);
                                                        string negDateFormat = negDate.Date.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
                                                        double price = Double.Parse(split[8]);
                                                        price = !ccy.Equals(ccy.ToUpper()) ? price * 100 : price;
                                                        string execStatus = ":KIND:CREATE";
                                                        string comment = "";

                                                        if (fOverallocStatuses.Contains(order.Workflow.WFStatus))
                                                        {
                                                            log.log(Severity.error, string.Format("The order has been already filled or overallocated.Execution will be cancelled."));
                                                            execStatus = ":KIND:CANCEL";
                                                            comment = ":INFO:CANCEL OVERALLOCATED EXECUTION";
                                                        }
                                                        else
                                                        {
                                                            // full exectuion - 0, partial execution - 1
                                                            if (sanityCheck)
                                                                comment = ":USERFIELD[MEDIO_PARTIAL_EXECUTION]:1";
                                                            else
                                                                comment = ":USERFIELD[MEDIO_PARTIAL_EXECUTION]:0";
                                                        }

                                                        string executionLine = ":TREF:" + split[17] + execStatus + ":QUTY:" + split[11] + ":BYSL:" + buyOrSale + ":PRCE:" + price + ":DNEG:" + negDateFormat + ":HNEG:" + timeResult + ":DSET:" + setlDateFormat + ":DVAL:" + setlDateFormat + ":CTRP:" + CSxRBCOrderAdapter.cptyForRBC + ":OREF:" + placementId + ":SCUR:" + ccy.ToUpper() + comment;


                                                        if (File.Exists(rmaSophisFile))
                                                        {
                                                            using (StreamWriter writer = new StreamWriter(rmaSophisFile, true))
                                                            {
                                                                writer.Write(executionLine);
                                                                writer.Write(writer.NewLine);
                                                            }
                                                        }

                                                        //WorkflowEventName eventToApply;
                                                        //if (CheckEventOnOrder(order, eventName, out eventToApply))
                                                        //{
                                                        //    RaiseEventOnOrder(order, eventToApply);
                                                        //}
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                stream.Close();
                            }

                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }

                        }
                        else if (RBCConfigurationSectionGroup.WfEventsSection.UseACKFiles==true)
                        if (fileExtension.ToUpper() == string.Format(".{0}", RBCConfigurationSectionGroup.RBCFileSection.NACKFileExtension).ToUpper()
                            || fileExtension.ToUpper() == string.Format(".{0}", RBCConfigurationSectionGroup.RBCFileSection.ACKFileExtension).ToUpper())
                        {
                            int orderId = GetOrderIdFromFile(file);
                            if (orderId != 0)
                            {
                                IOrder order = fOrderManager.GetOrderById(orderId);
                                if (order != null)
                                {
                                    log.log(Severity.debug, string.Format("Start sending new order message..."));
                                    var managerOA = OrderManagerConnector.Instance.GetOrderManagerForOA();
                                    WFStateInfo info = fOrderManager.GetState(order.ID);
                                    if (info != null)
                                    {
                                        for (int i = 0; i < info.AvailableEvents.Count; i++)
                                        {
                                            log.log(Severity.debug
                                                , string.Format("Order {0} in State [{1}/{2}] Event [{3}]/[{4}] Available"
                                                    , order.ID, order.Workflow.WFStatus, info, info.AvailableEvents[i].Name.ServiceName, info.AvailableEvents[i].Name.EventName));
                                        }
                                    }
                                    var orderPlacemendID = orderId;
                                    var orderPlacement = OrderManager.Instance.GetLastOrderPlacement(order.ID);
                                    if (orderPlacement != null)
                                    {
                                        orderPlacemendID = orderPlacement.Id;
                                        log.log(Severity.debug, string.Format("placement ID found for order {0} = {1}", orderId, orderPlacemendID));
                                    }
                                    else
                                        log.log(Severity.debug, string.Format("Cannot find the placement ID for order {0}. Use the orderId for the placementId.", orderId));

                                    var msgList = new List<OAMessage>();
                                    //NACK
                                    if (fileExtension.ToUpper() ==
                                        string.Format(".{0}", RBCConfigurationSectionGroup.RBCFileSection.NACKFileExtension).ToUpper())
                                    {
                                        var msg = new OAOrderRejectedMessage(0, orderPlacemendID, 0, "RBC NACK received");
                                        msgList.Add(msg);
                                    }
                                    //ACK
                                    else if (fileExtension.ToUpper() ==
                                             string.Format(".{0}",
                                                     RBCConfigurationSectionGroup.RBCFileSection.ACKFileExtension)
                                                 .ToUpper())
                                    {
                                        var msg = new OAOrderNewMessage(0, orderPlacemendID, order.ID.ToString());
                                        msgList.Add(msg);
                                    }

                                    managerOA.OnOAOrderMessage(msgList);
                                    log.log(Severity.debug, string.Format("Finished sending message"));

                                    if (File.Exists(file))
                                    {
                                        File.Delete(file);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.log(Severity.error, string.Format("Unexpected Error: [{0}]", e.Message));
                }

                log.log(Severity.debug, "End");
                log.end();
            }
        }

        private bool ExecutionSanityCheck(string[] execDetails, SingleOrder singleOrder)
        {
            Double execAmt = 0;
            Double execUnits = 0;
            bool isValid = false;
            double threshold = 0.003;
            EQuantityType amountType = singleOrder.QuantityData.OrderedType;
            var ccy = GetCcy(singleOrder.Target.Currency);

            if (amountType == EQuantityType.Amount)
            {
                if (Double.TryParse(execDetails[10], out execAmt))
                {
                    if (!ccy.ToUpper().Equals(ccy)) // GBp
                        execAmt *= 100;

                    if (Math.Abs(execAmt / singleOrder.QuantityData.OrderedQty - 1) > threshold)
                        isValid = true;
                }
            }
            else if (amountType == EQuantityType.Share)
            {
                if (Double.TryParse(execDetails[11], out execUnits))
                    if (Math.Abs(execUnits/singleOrder.QuantityData.OrderedQty -1) > threshold)
                        isValid = true;
            }
            else if (amountType == EQuantityType.Unit)
            {
                if (Double.TryParse(execDetails[11], out execUnits))
                {
                    var tradingUnits = GeTradingUnits(singleOrder.Target.SecurityID);
                    if (Math.Abs(execUnits / (singleOrder.QuantityData.OrderedQty * tradingUnits) -1) > threshold)
                        isValid = true;
                }
            }
            return isValid;
        }
     
        private bool RaiseEventOnOrder(IOrder order, WorkflowEventName evtToApply)
        {
            using (Logger log = new Logger(this, "raiseEventOneOrder"))
            {
                log.log(Severity.debug, "Start");
                bool result;

                try
                {
                    log.log(Severity.info, string.Format("Order {0} Raising Event [{1}]", order.ID, evtToApply));
                    fOrderManager.RaiseEvent(evtToApply, new List<IOrder>() { order }, null);
                    result = true;
                }
                catch (Exception e)
                {
                    log.log(Severity.error, string.Format("Unexpected Error for Order {0} Cannot Raise Event [{1}]: [{2}]", order.ID, evtToApply, e.Message));
                    result = false;
                }

                log.log(Severity.debug, "End");
                return result;
            }
        }

        private bool CheckEventOnOrder(IOrder order, string @event, out WorkflowEventName toApply)
        {
            using (Logger log = new Logger(this, "CheckEventOnOrder"))
            {
                log.log(Severity.debug, "Start");

                bool result;

                try
                {
                    WFStateInfo info = fOrderManager.GetState(order.ID);
                    if (info != null)
                    {
                        bool found = false;
                        for (int i = 0; i < info.AvailableEvents.Count && !found; i++)
                        {
                            if (info.AvailableEvents[i].Description == @event)
                            {
                                log.log(Severity.debug
                                    , string.Format("Order {0} in State [{1}/{2}] Event [{3}]/[{4}] Available"
                                    , order.ID, order.Workflow.WFStatus, info, info.AvailableEvents[i].Name.ServiceName, info.AvailableEvents[i].Name.EventName));
                                toApply = new WorkflowEventName(info.AvailableEvents[i].Name.ServiceName, info.AvailableEvents[i].Name.EventName);
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            log.log(Severity.debug, string.Format("Order {0} in State [{1}] Event [{2}] Valid", order.ID, info, @event));
                            result = true;
                        }
                        else
                        {
                            log.log(Severity.error, string.Format("Order {0} in State [{1}] Event [{2}] Not Valid", order.ID, info, @event));
                            result = false;
                        }
                    }
                    else
                    {
                        log.log(Severity.error, string.Format("Order {0} Cannot Get State", order.ID));
                        result = false;
                    }
                }
                catch (Exception e)
                {
                    log.log(Severity.error, string.Format("Order {0} Unexpected Error: [{1}]", order.ID, e.Message));
                    result = false;
                }

                log.log(Severity.debug, "End");
                log.end();

                return result;
            }
        }

        private int GetOrderIdFromFile(string file)
        {
            int orderId = 0;
            string filenameWithoutPath = System.IO.Path.GetFileName(file);
            using (Logger log = new Logger(this, "GetOrderIdFromFile"))
            {
                try
                {
                    string[] FileNameTokens = filenameWithoutPath.Split('_');
                    if (FileNameTokens.Length != 5)
                    {

                        log.log(Severity.error, string.Format("File tokens number: [{0}]", FileNameTokens.Length));
                        log.log(Severity.error, string.Format("File first token :[{0}]", FileNameTokens));
                        log.log(Severity.error, string.Format("File [{0}] does not have corresponding name format", filenameWithoutPath));
                    }
                    else
                    {
                        if (!Int32.TryParse(FileNameTokens[3], out orderId))
                            log.log(Severity.error, string.Format("File [{0}]: Order id not available", filenameWithoutPath));

                        else
                            return orderId;
                    }

                }
                catch (Exception e)
                {
                    log.log(Severity.error, string.Format("Unexpected Error for File [{0}]: {1}", filenameWithoutPath, e.Message));
                }
                return orderId;
            }

        }

        private void LoadOverallocStatusesFromConfig()
        {
            using (Logger log = new Logger(this, "LoadOverallocStatusesFromConfig"))
            {
                fOverallocStatuses.Clear();
                try
                {
                    string orderStatuses = RBCConfigurationSectionGroup.RBCFileSection.ExecutionRejectOrderStatuses;
                    log.log(Severity.info, string.Format("Loaded statuses list from config: [{0}] ", RBCConfigurationSectionGroup.RBCFileSection.ExecutionRejectOrderStatuses));


                    string[] statuses = orderStatuses.Split(';');
                    foreach (string item in statuses)
                    {
                        log.log(Severity.info, string.Format("Adding order status item in list: [{0}] ", item));

                        fOverallocStatuses.Add(item);
                    }
                }
                catch (Exception e)
                {
                    log.log(Severity.error, string.Format("Unexpected Error ", e.Message));
                }

            }

        }

        public string GetCcy(int ccy)
        {
            using (OracleCommand command = new OracleCommand())
            {
                string query = String.Format("select DEVISE_TO_STR({0}) from dual", ccy);
                command.Connection = Sophis.DataAccess.DBContext.Connection;
                command.CommandText = query;
                return command.ExecuteScalar().ToString();
            }
        }

        public double GeTradingUnits(int sicovam)
        {
            using (OracleCommand command = new OracleCommand())
            {
                string query = String.Format("select QUOTITE from titres where sicovam = {0}", sicovam);
                command.Connection = Sophis.DataAccess.DBContext.Connection;
                command.CommandText = query;
                return Double.Parse(command.ExecuteScalar().ToString());
            }
        }

        #endregion

    }

}
