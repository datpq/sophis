using NLog;
using Sophis.API;
using sophis.dataService.clientApi;
using sophis.dataService.contract;

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Threading;

namespace ToolkitService
{
    class DSRequest
    {
        public int TaskId { get; set; }
        public DateTime ExpiredTime { get; set; }
        public Action<string, string> CallBackFunc { get; set; }
    }

    public partial class ToolkitServiceMain : ServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static bool debugMode = false;
        private SophisRuntime sophisApi;
        private DataServiceClient sophisDataService;
        private List<StandardFileWatcher> filesWatchers = new List<StandardFileWatcher>();
        private System.Timers.Timer timer;
        private readonly object _tasksLock = new object();
        private List<DSRequest> TaskDSList = new List<DSRequest>();

        public ToolkitServiceMain()
        {
            InitializeComponent();
        }

        public void DoStart()
        {
            try
            {
                if (!debugMode)
                {
                    if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                    {
                        Logger.Info("Another instance is running. No multiple instances are allowed.");
                        return;
                    }
                }
            }
            catch (Exception e) { Logger.Error(e, "Error starting service"); }

            Initialize();
        }

        private void DoStop()
        {
            Logger.Info("Stopping the service...");
            try
            {
                DimOTCService.Stop();
                if (timer != null)
                {
                    Logger.Info("Stopping timer...");
                    timer.Stop();
                    timer.Dispose();
                }
                foreach(var fw in filesWatchers)
                {
                    Logger.Info($"Stopping fileWatcher {fw.Name}...");
                    fw.Stop();
                    fw.Dispose();
                }
                Logger.Info("Disconnecting the service...");
                sophisDataService.Disconnect();
                Logger.Info("Stopping the api...");
                sophisApi.Stop();
                if (debugMode)
                {
                    Logger.Info("Debug mode. Exiting...");
                    Environment.Exit(1);
                }
            }
            catch (Exception e) { Logger.Error(e, "Error starting service"); }
        }

        protected override void OnStart(string[] args)
        {
            DoStart();
        }

        protected override void OnStop()
        {
            Logger.Info("OnStop.BEGIN");
            DoStop();
            Logger.Info("OnStop.END");
        }

        private void Initialize()
        {
            try
            {
                Logger.Info("Initializing.BEGIN");
                sophisApi = new SophisRuntime();
                sophisApi.SetName("TOS");
                Logger.Info("Starting Sophis Api...");
                sophisApi.Start();

                sophisDataService = sophis.dataService.clientApi.DataServiceClient.Instance;
                Logger.Info("Connecting to DataService...");
                sophisDataService.Connect();

                var timerInterval = int.Parse(ConfigurationManager.AppSettings["DSTaskCheckInterval"]);
                timer = new System.Timers.Timer(timerInterval*1000);
                timer.Elapsed += Timer_Elapsed;
                timer.AutoReset = true;
                timer.Enabled = true;

                string inputFolder = ConfigurationManager.AppSettings["InputFolder"];
                string fileFilter = ConfigurationManager.AppSettings["FileFilter"];
                filesWatchers.Add(new StandardFileWatcher("Main", inputFolder, fileFilter, ProcessFileMain));
                DimOTCService.CreateInstrument = ProcessLine;
                filesWatchers.Add(new StandardFileWatcher("DOS_Blotter", ConfigurationManager.AppSettings["DIM_OTC_InputFolder"],
                    ConfigurationManager.AppSettings["DIM_OTC_FilterCsv"], DimOTCService.ProcessBlotterFile));
                filesWatchers.Add(new StandardFileWatcher("DOS_FileConnector", ConfigurationManager.AppSettings["DIM_OTC_InputFolder"],
                    ConfigurationManager.AppSettings["DIM_OTC_FilterXml"], DimOTCService.ProcessFileConnectorFile));
            }
            catch (Exception e) {
                Logger.Error(e, "Error initializing service");
                throw e;
            } finally {
                Logger.Info("Initializing.END");
            }
        }

        private static List<DataUpdateStatus> CompletedStatus = new List<DataUpdateStatus> {
            DataUpdateStatus.Error, DataUpdateStatus.Aborted, DataUpdateStatus.DataUpdated, DataUpdateStatus.NoProviderDataChange,
            DataUpdateStatus.Conflict, DataUpdateStatus.ConflictAccepted, DataUpdateStatus.ConflictError, DataUpdateStatus.ConflictRefused };
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //Logger.Info("Timer_Elapsed.BEGIN");
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            lock(_tasksLock)
            {
                for(int i=TaskDSList.Count-1; i>=0; i--)
                {
                    var request = TaskDSList[i];
                    Logger.Info($"Checking for task {request.TaskId}");
                    var taskDS = TaskManager.GetTask(request.TaskId);
                    Logger.Info($"DataStatus={taskDS.DataStatus}");
                    if (CompletedStatus.Contains(taskDS.DataStatus))
                    {
                        Logger.Info($"Task finished. Sending update to the caller.");
                        request.CallBackFunc?.Invoke(request.TaskId.ToString(), taskDS.DataStatus.ToString());
                        TaskDSList.RemoveAt(i);
                    } else if (DateTime.Now >request.ExpiredTime)
                    {
                        Logger.Warn($"Waiting time expired. Don't check for status anymore.");
                        request.CallBackFunc?.Invoke(request.TaskId.ToString(), taskDS.DataStatus.ToString());
                        TaskDSList.RemoveAt(i);
                    }
                }
            }
            //Logger.Info("Timer_Elapsed.BEGIN");
        }

        private void ProcessFileMain(string inputCsvFile)
        {
            string[] lines = File.ReadAllLines(inputCsvFile);
            Logger.Info("Moving file to done");
            Utils.MoveToDestFile(inputCsvFile, $"{inputCsvFile}.done");

            if (lines.Length > 0 && lines[0].Equals("Exit", StringComparison.InvariantCultureIgnoreCase))
            {
                Logger.Info("Receiving Exit...");
                DoStop();
                return;
            }
            for(int i=1; i<lines.Length; i++)
            {
                try
                {
                    var inputLine = lines[i];
                    Logger.Info($"processing line {i}/{lines.Length - 1}: {inputLine}");
                    var output = ProcessLine(inputLine);
                } catch(Exception e)
                {
                    Logger.Error(e, $"Error when processing line {i}");
                }
            }
        }

        private string ProcessLine(string inputLine, Action<string, string> callBackFunc = null)
        {
            string ans = null;
            var vals = inputLine.Split(';');
            if (vals[0] == "CreateInstrument")//CreateInstrument;ExternalReferencesValue;ExternalReferencesType;DataSource;DataType;SecurityType;ResourceType;Creator
            {
                Logger.Info("CreateInstrument...");
                var task = new Task();
                task.Entity = new UpdateEntity();
                task.Entity.ExternalReferences = new List<Reference> {
                            new Reference {
                                Value = vals[1],
                                Type = vals[2] // "ISIN"
                            }
                        };
                task.Entity.DataSource = vals[3]; // "BBG-Adhoc";
                task.Entity.DataType = vals[4]; // "Definition";
                task.Entity.SecurityType = vals[5]; // "Equity";
                task.Entity.ResourceType = vals[6]; // "Instrument";
                task.Entity.ConflictHandlingBehaviour = ConflictHandlingBehaviour.Auto;
                task.Entity.UpdateMode = UpdateMode.RequestAndNonExistingChildRequests;
                var userRights = new CSMUserRights(vals[7]);
                task.Creator = new User
                {
                    Name = userRights.GetName(), //"DSAdmin"
                    SophisID = userRights.GetIdent()
                };
                var taskResult = TaskManager.SaveTasks(new List<Task> { task });
                var taskID = taskResult[0].Id;
                Logger.Info($"Task ID : {taskID}. Executing...");
                TaskManager.ExecuteTasks(new int[] { taskID });
                lock (_tasksLock)
                {
                    Logger.Info($"Adding task {taskID} for monitoring");
                    TaskDSList.Add(new DSRequest
                    {
                        TaskId = taskID,
                        CallBackFunc = callBackFunc,
                        ExpiredTime = DateTime.Now.AddMinutes(int.Parse(ConfigurationManager.AppSettings["DSTimeout"]))
                    });
                }
                ans = taskID.ToString();
            }
            else
            {
                Logger.Error($"Skip unknown command: {vals[0]}");
            }
            return ans;
        }
    }
}
