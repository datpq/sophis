using System.ComponentModel;
using sophis.reporting;
using Sophis.Reporting.Controls.CommonTypes;

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public class CalendarParameterTypeSetting : CSMParameterTypeSettingGUI
        {
            private readonly UcCalendarSetting _ucCalendarSetting = new UcCalendarSetting(); 

            public override void ValidateSettingParameter(CSMParameter iparam)
            {
                _ucCalendarSetting.ValidateSettingParameter(iparam);
            }

            public override Component GetControl(CSMParameter iparam)
            {
                _ucCalendarSetting.Initialize(iparam);
                return _ucCalendarSetting;
            }
        }
    }
}
