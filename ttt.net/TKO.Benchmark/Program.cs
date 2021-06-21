using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TTT;

namespace TKO.Benchmark
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Logger.Debug("main.BEGIN");

            //var Settings = Transformation.InitializeAndSaveConfig();
            var Settings = Transformation.LoadConfigFromFile();

            Transformation.Transform(Settings, TransName.YieldCurve.ToString(), 1, "yieldcurve.csv", "output.xml");

            Logger.Debug("main.END");
        }
    }
}
