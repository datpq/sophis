using NLog;
using System;
using System.IO;
using System.Linq;

namespace ToolkitService
{
    public class StandardFileWatcher : FileSystemWatcher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private Action<string> ProcessFile;
        public string Name { get; set; }

        public StandardFileWatcher(string name, string path, string filter, Action<string> processFile)
            : base()
        {
            Name = name;
            Logger.Info($"SFW.{Name}.BEGIN(path={path}, filter={filter})");
            if (!System.IO.Path.IsPathRooted(path))
            {
                path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), path);
            }
            if (!Directory.Exists(path))
            {
                Logger.Info(string.Format("Creating directory: {0}", path));
                Directory.CreateDirectory(path);
            }

            Logger.Info("Process file at the start of watcher...");
            var fileEntries = Directory.GetFiles(path, filter);
            foreach (var file in fileEntries)
            {
                Logger.Info($"Processing {file}...");
                processFile(file);
            }

            ProcessFile = processFile;
            Path = path;
            NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;
            //watcher.NotifyFilter = NotifyFilters.LastWrite;
            Filter = filter;
            //watcher.Changed += OnChanged;
            Created += OnCreated;
            Renamed += OnRenamed;
            EnableRaisingEvents = true;
            Logger.Info($"SFW.{Name}.END");
            Logger.Info($"SFW.{Name}. Waiting for files to process: {path}\\{filter}...");
        }

        public void Stop()
        {
            Created -= OnCreated;
            Renamed -= OnRenamed;
            EnableRaisingEvents = false;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Logger.Info($"{Name}.OnCreated.BEGIN(FullPath={e.FullPath})");
            Process(e.FullPath);
            Logger.Info($"{Name}.OnCreated.END");
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            if (Directory.GetFiles(Path, Filter).Any(
                x => x.Equals(e.FullPath, StringComparison.InvariantCultureIgnoreCase)))
            {
                Logger.Info($"{Name}.OnRenamed.BEGIN(FullPath={e.FullPath})");
                Process(e.FullPath);
                Logger.Info($"{Name}.OnRenamed.END");
            }
            else
            {
                //Logger.Warn($"File {e.FullPath} already processed.");
            }
        }

        private void Process(string filePath)
        {
            if (!Utils.IsFileClosed(filePath))
            {
                Logger.Error($"{Name}.Can not open file: {filePath}");
                return;
            }
            ProcessFile(filePath);
        }
    }
}
