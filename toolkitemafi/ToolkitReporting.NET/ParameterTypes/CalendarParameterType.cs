using System;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using Sophis.Reporting.Controls.CommonTypes;
using Eff.Utils;

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public class CalendarParameterType : AbstractParameterType
        {
            public const int ControlWidth = 240;

            public CalendarParameterType() : base(ControlWidth)
            {
            }

            protected override Control CreateControl(CSMGridParameter iparam)
            {
                try
                {
                    var calendarControl = new DateTimePicker();
                    var formula = iparam.ApiParameter.GetSettingFormula();
                    calendarControl.Enabled = string.IsNullOrEmpty(formula); //make control disabled if it's a calculated parameter
                    var format = iparam.ApiParameter.GetSettingFormat();
                    if (string.IsNullOrEmpty(format)) //if no format defined --> take the default format (token DATE_FORMAT)
                    {
                        format = Utils.TokenDateFormat.Replace('D', 'd').Replace('Y', 'y');//sql format to .NET format
                    }
                    if (!string.IsNullOrEmpty(iparam.Value))
                    {
                        try{
                            calendarControl.Value = DateTime.ParseExact(iparam.Value, format, null);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    //on change event, update the value of parameter, and also the calculated paramters that depend on
                    calendarControl.ValueChanged += (sender, args) =>
                    {
                        try
                        {
                            var dateTimePicker = sender as DateTimePicker;
                            if (dateTimePicker == null) return;

                            //update the parameter's value
                            iparam.Value = dateTimePicker.Value.ToString(format);

                            var csmAskParameters = Utils.GetAskParametersDlg();
                            //find calculated parameters that depend on this parameter
                            foreach (var calculatedParam in csmAskParameters.GetParameters().Where(x => (x.Type == iparam.Type) && (x.Name != iparam.Name)))
                            {
                                var calculatedFormula = calculatedParam.ApiParameter.GetSettingFormula();
                                EmcLog.Debug("calculatedParam={0}, calculatedFormula={1}", calculatedParam.Name, calculatedFormula);
                                //System.Diagnostics.Debug.WriteLine("calculatedParam={0}, calculatedFormula={1}", calculatedParam.Name, calculatedFormula);
                                if (calculatedFormula.Contains(string.Format("[{0}]", iparam.Name))) //main parameter found in calculated formula
                                {
                                    //System.Diagnostics.Debug.WriteLine("calculatedFormula={0}", calculatedFormula);
                                    var sqlQuery = string.Format("SELECT {0} FROM DUAL",
                                        calculatedFormula.Replace(string.Format("[{0}]", iparam.Name),
                                            string.Format("TO_DATE('{0}', 'YYYYMMDD')", dateTimePicker.Value.ToString("yyyyMMdd"))));

                                    var connection = Sophis.DataAccess.DBContext.Connection;
                                    var calculatedDateTime = connection.ExecuteScalar<DateTime>(sqlQuery, null, null, null, null);
                                    EmcLog.Debug("sqlQuery={0}, calculatedDateTime={1}", sqlQuery, calculatedDateTime);
                                    //System.Diagnostics.Debug.WriteLine("sqlQuery={0}, calculatedDateTime={1}", sqlQuery, calculatedDateTime);

                                    //set the value of calculated parameter
                                    var calculatedFormat = calculatedParam.ApiParameter.GetSettingFormat();
                                    if (string.IsNullOrEmpty(calculatedFormat)) //if no format defined --> take the default format (token DATE_FORMAT)
                                    {
                                        calculatedFormat = Utils.TokenDateFormat.Replace('D', 'd').Replace('Y', 'y');//sql format to .NET format
                                    }
                                    calculatedParam.Value = calculatedDateTime.ToString(calculatedFormat);
                                    //System.Diagnostics.Debug.WriteLine("calculatedParam={0}", calculatedParam.Value);

                                    //set the value of the control
                                    var mapParamControl = MapParamControl;
                                    if (mapParamControl.ContainsKey(calculatedParam))
                                    {
                                        ((DateTimePicker) mapParamControl[calculatedParam]).Value = calculatedDateTime;
                                        //System.Diagnostics.Debug.WriteLine("control of calculated parameter found, set value={0}", calculatedDateTime);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            EmcLog.Error(e.ToString());
                            MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };
                    return calendarControl;
                }
                catch (Exception e)
                {
                    EmcLog.Error(e.ToString());
                    MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }
    }
}
