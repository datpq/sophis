using System.Windows.Forms;
using sophis.reporting;

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public partial class UcCalendarSetting : UserControl
        {
            public UcCalendarSetting()
            {
                InitializeComponent();
            }

            public void Initialize(CSMParameter param)
            {
                txtFormat.Text = param.GetSettingFormat();
                txtFormula.Text = param.GetSettingFormula();
            }

            public void ValidateSettingParameter(CSMParameter param)
            {
                var setting = param.GetSettingParameters();
                setting.Clear();
                if (txtFormat.Text != string.Empty)
                {
                    setting.Add(Utils.SettingFormat, txtFormat.Text);
                }
                if (txtFormula.Text != string.Empty)
                {
                    setting.Add(Utils.SettingFormula, txtFormula.Text);
                }
            }
        }
    }
}