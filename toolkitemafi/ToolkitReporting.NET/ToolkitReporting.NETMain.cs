using System;
using System.Threading;
using System.Windows.Forms;

using sophis;
using sophis.scenario;
using Eff.Utils;

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        /// <summary>
        /// Definition of DLL entry point: to register new functionality and closing point
        /// </summary>
        public class MainClass : IMain
        {
            public static readonly string Caption = "Expresso Reporting";
            private readonly Thread _askParametersMonitoringThread = new Thread(Utils.ScanAskParameters);

            public void EntryPoint()
            {
                //{{SOPHIS_INITIALIZATION (do not delete this line)
                EmcLog.Debug("BEGIN");
                try
                {
                    CompositionRoot.Wire(new ApplicationModule());
                    // TO DO; Perform registrations
                    Utils.Init();
                    AbstractParameterType.InitializeAllParameterTypes();
                    //BindingFunctionImpl.Initialize();

                    CSMScenario.Register(Caption, new ReportingScenario());
                    _askParametersMonitoringThread.Start();
                }
                catch (Exception e)
                {
                    EmcLog.Error(e.ToString());
                    MessageBox.Show(string.Format("Error: {0}", e.Message), Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                EmcLog.Debug("END");

			//}}SOPHIS_INITIALIZATION
            }

            public void Close()
            {
                EmcLog.Debug("BEGIN");
                Utils.StopAskParam = true;
                _askParametersMonitoringThread.Join();
                EmcLog.Debug("END");
            }
        }
    }
}
