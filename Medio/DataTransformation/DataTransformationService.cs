using NLog;
using DataTransformation.Models;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using DataTransformation.Settings;
using System.ServiceProcess;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Oracle.DataAccess.Client;
using IniParser;
using IniParser.Model;

namespace DataTransformation
{
    partial class DataTransformationService : ServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static PdtTransformationSetting Setting;
        private readonly static object syncLock = new object();
        private static int fileSeq = 1;
        private static int ssbSeq = 1;
        public const string DEFAULT_CONFIG = "dt.xml";
        private const string STORAGE = "storage.txt";
        public static OracleConnection DbConnection;
        List<TransformationWatcher> watchers = new List<TransformationWatcher>();

        public DataTransformationService()
        {
            InitializeComponent();
        }

        public void Start()
        {
            try
            {
                if (System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
                {
                    Logger.Debug("Another instance is running. No multiple instances are allowed.");
                    return;
                }
                var storageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STORAGE);
                if (File.Exists(storageFilePath))
                {
                    var parser = new FileIniDataParser();
                    IniData data = parser.ReadFile(storageFilePath);
                    try { fileSeq = int.Parse(data["Sequences"]["FileSeq"]); }
                    catch (Exception e) { Logger.Error(e, "Error retrieving FileSeq"); }
                    try { ssbSeq = int.Parse(data["Sequences"]["SSBSeq"]); }
                    catch (Exception e) { Logger.Error(e, "Error retrieving SSBSeq"); }
                    Logger.Debug($"Got sequence from storage file FileSeq={fileSeq}, ssbSeq={ssbSeq}");
                }
            }
            catch (Exception e) { Logger.Error(e, "Error starting service"); }

            Initialize();

            //Transformation.Transform2Xml(Setting, TransName.YieldCurve.ToString(), 1, "yieldcurve.csv", "output.xml");
            //Transformation.Transform2Xml(Setting, TransName.Benchmark.ToString(), 1, "input_benchmark.csv", "output.xml");

            Logger.Info("Waiting for files to process...");
            //Thread.Sleep(Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        protected override void OnStop()
        {
            try
            {
                Logger.Info("Stopping the watchers...");
                foreach (var watcher in watchers)
                {
                    watcher.Stop();
                    watcher.Dispose();
                }
                watchers.Clear();
                DbConnection.Close();
                Logger.Info("Finished.");
            }
            catch (Exception e)
            {
                Logger.Info("OnStop Error {0}", e.Message);
            }
        }

        private static string PrepareDir(string dirPath)
        {
            if (!Path.IsPathRooted(dirPath))
            {
                dirPath = AppDomain.CurrentDomain.BaseDirectory + dirPath;
            }
            if (!Directory.Exists(dirPath))
            {
                Logger.Info(string.Format("Creating directory: {0}", dirPath));
                Directory.CreateDirectory(dirPath);
            }
            return dirPath;
        }

        private void Initialize()
        {
            Logger.Info("Initializing.BEGIN");

            //Setting = Transformation.InitializeAndSaveConfig(DEFAULT_CONFIG);
            Setting = Transformation.LoadConfigFromFile(DEFAULT_CONFIG);

            //SQL Connection parameters
            var connectionStrings = ConfigurationManager.AppSettings["OracleConnectionString"];
            DbConnection = new OracleConnection(connectionStrings);
            try
            {
                DbConnection.Open();
                Logger.Info("Open connection to Database");
            }
            catch (Exception ex)
            {
                Logger.Info("Oracle Connection Error {0}", ex.Message);
            }

            var transformationSettings = ConfigurationManager.GetSection("TransformationSettings") as TransformationIOConfigurationSection;
            if (transformationSettings == null)
            {
                Logger.Error("Transformation Settings are not defined");
                return;
            }
            foreach (TransformationIO transIO in transformationSettings.TransIOs)
            {
                Logger.Info(string.Format("Transformation Name={0}, Type={1}", transIO.Name, transIO.Type));
                Logger.Info(string.Format("InputDir={0}, InputFilter={1}, OutputDir={2}, OutputFile={3}, BackupDir={4}, FailureDir={5}",
                    transIO.InputDir, transIO.InputFilter, transIO.OutputDir, transIO.OutputFile, transIO.BackupDir, transIO.FailureDir));
                var fileEntries = Directory.GetFiles(transIO.InputDir, transIO.InputFilter);
                foreach (var file in fileEntries)
                {
                    Process(file, transIO);
                }
                watchers.Add(new TransformationWatcher(transIO));
            }

            Logger.Info("Initializing.END");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Process(string filePath, TransformationIO transIO)
        {
            //lock(syncLock)
            //{
            Logger.Debug(string.Format("Process.BEGIN(filePath={0}, Name={1})", filePath, transIO.Name));

            var subfix = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFile = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(filePath), subfix, Path.GetExtension(filePath));
            try
            {
                Logger.Debug(string.Format("Backuping file to {0}\\{1}...", transIO.BackupDir, backupFile));
                if (Directory.Exists(transIO.BackupDir))
                {
                    File.Copy(filePath, Path.Combine(transIO.BackupDir, backupFile));
                }
                else
                {
                    File.Copy(filePath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, transIO.BackupDir, backupFile));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error while backuping file");
            }

            try
            {
                var outputFile = string.IsNullOrEmpty(transIO.OutputFile) ? Path.GetFileName(filePath) : transIO.OutputFile;
                outputFile = outputFile.Replace("@InputFile", Path.GetFileNameWithoutExtension(filePath));
                if (outputFile.Contains("@FileSeq") || outputFile.Contains("@SSBSeq"))
                {
                    if (outputFile.Contains("@FileSeq"))
                    {
                        while(File.Exists(Path.Combine(transIO.OutputDir, outputFile.Replace("@FileSeq", fileSeq.ToString("D3"))))
                            || File.Exists(Path.Combine(transIO.OutputDir, outputFile.Replace("@FileSeq", fileSeq.ToString("D3")) + ".processed")))
                        {
                            Logger.Warn($"File exists already. Increasing @FileSeq: {fileSeq}-->{fileSeq+1} " + outputFile.Replace("@FileSeq", fileSeq.ToString("D3")));
                            Interlocked.Increment(ref fileSeq);
                        }
                        outputFile = outputFile.Replace("@FileSeq", fileSeq.ToString("D3"));
                        Interlocked.Increment(ref fileSeq);
                        //fileSeq++;
                        //File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STORAGE), fileSeq.ToString());
                    }
                    if (outputFile.Contains("@SSBSeq"))
                    {
                        while (File.Exists(Path.Combine(transIO.OutputDir, outputFile.Replace("@SSBSeq", ssbSeq.ToString("D3"))))
                            || File.Exists(Path.Combine(transIO.OutputDir, outputFile.Replace("@SSBSeq", ssbSeq.ToString("D3")) + ".processed")))
                        {
                            Logger.Warn($"File exists already. Increasing @SSBSeq: {ssbSeq}-->{ssbSeq + 1} " + outputFile.Replace("@SSBSeq", ssbSeq.ToString("D3")));
                            Interlocked.Increment(ref ssbSeq);
                        }
                        outputFile = outputFile.Replace("@SSBSeq", ssbSeq.ToString("D3"));
                        Interlocked.Increment(ref ssbSeq);
                    }
                    var storageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, STORAGE);
                    var parser = new FileIniDataParser();
                    IniData data = File.Exists(storageFilePath) ? parser.ReadFile(storageFilePath) : new IniData();
                    data["Sequences"]["FileSeq"] = fileSeq.ToString();
                    data["Sequences"]["SSBSeq"] = ssbSeq.ToString();
                    parser.WriteFile(storageFilePath, data);
                }
                var failureFile = Path.GetFileNameWithoutExtension(outputFile) + "_FAILURE" + Path.GetExtension(outputFile);
                Transformation.Transform(transIO, Setting, transIO.Type, filePath, Path.Combine(transIO.OutputDir, outputFile), Path.Combine(transIO.FailureDir, failureFile));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error while tranforming file");
            }

            try
            {
                var processedFile = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}_processed{Path.GetExtension(filePath)}");
                if (!File.Exists(processedFile))
                {
                    Logger.Debug($"Renaming file to {processedFile}...");
                    File.Move(filePath, processedFile);
                } else
                {
                    int count = 1;
                    while (count <= 1000)
                    {
                        processedFile = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(filePath)}_processed_{count++}{Path.GetExtension(filePath)}");
                        if (!File.Exists(processedFile))
                        {
                            Logger.Debug($"Renaming file to {processedFile}...");
                            File.Move(filePath, processedFile);
                            break;
                        }
                    }
                    if (count > 1000)
                    {
                        Logger.Debug($"No more processed space is free. We are forced to delete the file: {filePath}");
                        File.Delete(filePath);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error while renaming file to processed");
            }

            Logger.Debug("Process.END");
            //}
        }
    }

    class TransformationWatcher : FileSystemWatcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TransformationIO TransIO { get; set; }

        public TransformationWatcher(TransformationIO transIO)
            : base()
        {
            TransIO = transIO;
            Path = transIO.InputDir;
            NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
            //watcher.NotifyFilter = NotifyFilters.LastWrite;
            Filter = transIO.InputFilter;
            //watcher.Changed += OnChanged;
            Created += OnCreated;
            Renamed += OnRenamed;
            EnableRaisingEvents = true;
        }

        public void Stop()
        {
            Created -= OnCreated;
            Renamed -= OnRenamed;
            EnableRaisingEvents = false;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Logger.Debug(string.Format("OnCreated.BEGIN(FullPath={0})", e.FullPath));
            DataTransformationService.Process(e.FullPath, (sender as TransformationWatcher).TransIO);
            Logger.Debug("OnCreated.END");
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (Directory.GetFiles(TransIO.InputDir, TransIO.InputFilter).Any(
                x => x.Equals(e.FullPath, StringComparison.InvariantCultureIgnoreCase)))
            {
                Logger.Debug(string.Format("OnRenamed.BEGIN(FullPath={0})", e.FullPath));
                DataTransformationService.Process(e.FullPath, (sender as TransformationWatcher).TransIO);
                Logger.Debug("OnRenamed.END");
            }
            else
            {
                //Logger.Warn($"File {e.FullPath} already processed.");
            }
        }

        //private static void OnChanged(object sender, FileSystemEventArgs e)
        //{
        //    if (e.ChangeType != WatcherChangeTypes.Changed)
        //    {
        //        return;
        //    }
        //    Logger.Info($"OnChanged.BEGIN(FullPath={e.FullPath})");
        //    Process(e.FullPath, (sender as TransformationWatcher)?.TransIO);
        //    Logger.Info("OnChanged.END");
        //}

    }
}
