using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTT.Models;
using NLog;
using System.Reflection;

namespace TTT
{
    public class TTTMain
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            Logger.Debug("main.BEGIN");

            //var Settings = Transformation.InitializeAndSaveConfig();
            var Settings = Transformation.LoadConfigFromFile();

            //Transformation.Transform(Settings, TransName.YieldCurve.ToString(), 1, "yieldcurve.csv", "output.xml");
            Transformation.Transform(Settings, TransName.Benchmark.ToString(), 1, "input_benchmark.csv", "output.xml");
            Logger.Debug("main.END");
        }
    }
}
