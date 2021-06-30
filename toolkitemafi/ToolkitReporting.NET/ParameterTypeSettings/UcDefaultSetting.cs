using System.Windows.Forms;
using sophis.reporting;

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public partial class UcDefaultSetting : UserControl
        {
            public UcDefaultSetting()
            {
                InitializeComponent();
            }

            public void Initialize(CSMParameter param)
            {
                txtDefaultValue.Text = param.GetSettingDefault();
            }

            public void ValidateSettingParameter(CSMParameter param)
            {
                var setting = param.GetSettingParameters();
                setting.Clear();
                if (txtDefaultValue.Text != string.Empty)
                {
                    setting.Add(Utils.SettingDefault, txtDefaultValue.Text);
                }
            }
        }
    }
}