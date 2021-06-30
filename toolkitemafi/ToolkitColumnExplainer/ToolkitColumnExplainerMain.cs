using System;
using System.Windows.Forms;
using sophis;
using sophis.utils;
using Eff.Utils;
using sophis.scenario;
using Eff.Utils;

///{{SOPHIS_TOOLKIT_INCLUDE (do not delete this line)

//}}SOPHIS_TOOLKIT_INCLUDE


namespace Eff
{
    namespace Utils
    {
        /// <summary>
        /// Definition of DLL entry point: to register new functionality and closing point
        /// </summary>
        public class MainClass : IMain
        {
            public static readonly string CAPTION = "ToolkitColumnExplainer";

            public void EntryPoint()
            {
                //{{SOPHIS_INITIALIZATION (do not delete this line)
                try
                {
                    CSMScenario.Register("Column Explainer", new ColumnExplainerScenario());
                    ColumnExplainer.ReloadFromDatabase();
                }
                catch (Exception e)
                {
                    EmcLog.Error(e.ToString());
                    MessageBox.Show(string.Format("Error: {0}", e.Message), CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
			//}}SOPHIS_INITIALIZATION
            }

            public void Close()
            {

            }
        }
    }
}
