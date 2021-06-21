using System;
using System.Configuration;
using System.IO;
using System.Linq;
using NLog;
using System.Security.Permissions;
namespace TKOPriceFeed
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main()
        {
            Logger.Info("Initializing...");
            Run();
            Console.ReadLine();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            var backupDir = AppDomain.CurrentDomain.BaseDirectory + "Backup";
            var doneDir = AppDomain.CurrentDomain.BaseDirectory + "Done";
            (new[] { backupDir, doneDir }).Where(x => !Directory.Exists(x)).ToList().ForEach(x =>
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Creating directory: {0}", x);
                }
                Directory.CreateDirectory(x);
            });

            var backupSuffix = ConfigurationManager.AppSettings["BackupSuffix"];
            var outputDir = ConfigurationManager.AppSettings["OutputDir"];

            //SQL Connection parameters
            var connectionStrings = ConfigurationManager.AppSettings["OracleConnectionString"];
            var fileIdx = 1;
            while (true)
            {
                var fileFilter = ConfigurationManager.AppSettings[string.Format("File{0}", fileIdx)];
                if (string.IsNullOrEmpty(fileFilter))
                {
                    break;
                }
                Logger.Info("Watching for the file: {0}", fileFilter);
                var watcher =
                    new PriceFeedWatcher()
                    {
                        ConnectionStrings = connectionStrings,
                        BackupDir = backupDir,
                        BackupSuffix = backupSuffix,
                        DoneDir = doneDir,
                        OutputDir = outputDir,
                        Path = Path.GetDirectoryName(fileFilter),
                        Filter = Path.GetFileName(fileFilter),
                        NotifyFilter = NotifyFilters.FileName,
                        XmlTemplate = Path.Combine("XmlTemplates", ConfigurationManager.AppSettings[string.Format("XmlTemplate{0}", fileIdx)]),
                        RepeatingNode = ConfigurationManager.AppSettings[string.Format("RepeatingNode{0}", fileIdx)]
                    };

                watcher.Renamed += PriceFeedWatcher_OnRenamed;
                watcher.Created += PriceFeedWatcher_OnCreated;
                Directory.GetFiles(watcher.Path, watcher.Filter).ToList().ForEach(file => watcher.Processing(file));
                watcher.EnableRaisingEvents = true;
                fileIdx++;
            }
        }


        private static void PriceFeedWatcher_OnRenamed(object sender, RenamedEventArgs e)
        {
            var watcher = (PriceFeedWatcher)sender;
            if (!Directory.GetFiles(watcher.Path, watcher.Filter).Any(file => file.ToUpper().Equals(e.FullPath.ToUpper()))) return;
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("A file renamed : {0}", e.FullPath);
            }
            watcher.Processing(e.FullPath);
        }

        private static void PriceFeedWatcher_OnCreated(object source, FileSystemEventArgs e)
        {
            var watcher = (PriceFeedWatcher)source;
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("A file created: {0}", e.FullPath);
            }
            watcher.Processing(e.FullPath);
        }
    }
}
