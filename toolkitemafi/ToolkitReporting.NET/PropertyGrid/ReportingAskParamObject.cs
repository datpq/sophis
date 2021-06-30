using System;
using System.Windows.Forms;
using Eff.Utils;
using sophis.reporting;

// ReSharper disable once CheckNamespace
namespace Eff.ToolkitReporting.NET.PropertyGrid
{
    public class ReportingAskParamObject : PropertyGridObject
    {
        private readonly string _reportTemplateName;

        public ReportingAskParamObject(string reportTemplateName)
        {
            try
            {
                EmcLog.Debug("BEGIN");
                _reportTemplateName = reportTemplateName;
                using (var reportTemplateManager = CSMReportTemplateManager.GetInstance())
                {
                    using (var reportTemplate = reportTemplateManager.GetReportTemplateWithName(reportTemplateName))
                    {
                    }
                }
            }
            catch (Exception e)
            {
                EmcLog.Error(e.ToString());
                MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.Caption, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                EmcLog.Debug("END");
            }
        }
    }
}
