using System.ServiceProcess;
using System.Threading;
using NLog;

namespace DataTransformation
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            Logger.Debug("Starting the service...");
            var servicesToRun = new ServiceBase[] 
			{ 
				new DataTransformationService() 
			};

            if ((args.Length == 1) && (args[0] == "noservice"))
            {
                DataTransformationService.debugMode = true;
                (servicesToRun[0] as DataTransformationService).Start();
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}
