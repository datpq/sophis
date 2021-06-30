using System.ComponentModel;
using sophis.reporting;
using Sophis.Reporting.Controls.CommonTypes;

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public class DefaultParameterTypeSetting : CSMParameterTypeSettingGUI
        {
            private readonly UcDefaultSetting _ucDefaultSetting = new UcDefaultSetting();

            public override void ValidateSettingParameter(CSMParameter iparam)
            {
                _ucDefaultSetting.ValidateSettingParameter(iparam);
            }

            public override Component GetControl(CSMParameter iparam)
            {
                _ucDefaultSetting.Initialize(iparam);
                return _ucDefaultSetting;
            }
        }
    }
}