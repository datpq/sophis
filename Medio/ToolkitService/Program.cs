using NLog;
using System.ServiceProcess;
using System.Threading;

namespace ToolkitService
{
    static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            Logger.Info("Starting the service...");
            var servicesToRun = new ServiceBase[]
            {
                new ToolkitServiceMain()
            };
            if ((args.Length == 1) && (args[0] == "noservice"))
            {
                ToolkitServiceMain.debugMode = true;
                (servicesToRun[0] as ToolkitServiceMain).DoStart();
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
