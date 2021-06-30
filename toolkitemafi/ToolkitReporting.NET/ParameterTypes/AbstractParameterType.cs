using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using Sophis.Reporting.Controls.CommonTypes;
using sophis.misc;
using Eff.Utils;

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public abstract class AbstractParameterType : CSMParameterTypeGUI
        {
            public const string ComboboxTypeListEntryName = "ParameterTypeComboboxList";
            public const string ComboboxTypePrefix = "ParameterTypeCombobox{0}";
            protected int MControlWidth;

            protected AbstractParameterType(int controlWidth)
            {
                MControlWidth = controlWidth;
            }

            protected Dictionary<CSMGridParameter, Control>  MapParamControl
            {
                get
                {
                    var csmAskParameters = Utils.GetAskParametersDlg();
                    if (csmAskParameters.Tag == null)
                    {
                        csmAskParameters.Tag = new Dictionary<CSMGridParameter, Control>();
                    }
                    var result = csmAskParameters.Tag as Dictionary<CSMGridParameter, Control>;
                    return result;
                }
                set
                {
                    var csmAskParameters = Utils.GetAskParametersDlg();
                    csmAskParameters.Tag = value;
                }
            }

            protected abstract Control CreateControl(CSMGridParameter iparam);

            public static void InitializeAllParameterTypes()
            {
                //register combobox parameter types
                string parameterTypeList = null;
                CSMConfigurationFile.getEntryValue(Utils.SectionName, ComboboxTypeListEntryName, ref parameterTypeList, "");
                foreach (var paramName in parameterTypeList.Split(','))
                {
                    string query = null;
                    CSMConfigurationFile.getEntryValue(Utils.SectionName, string.Format(ComboboxTypePrefix, paramName), ref query, "");
                    if (!string.IsNullOrEmpty(query))
                    {
                        Register(paramName, new ComboBoxParameterType(query));
                    }
                }

                //register other parameter types
                Register("Calendar", new CalendarParameterType());
                CSMParameterTypeSettingGUI.Register("Calendar", new CalendarParameterTypeSetting());

                foreach (var paramType in Utils.DefaultValueParamTypes)
                {
                    CSMParameterTypeSettingGUI.Register(paramType, new DefaultParameterTypeSetting());
                }
            }

            public override Component GetControl(CSMGridParameter iparam)
            {
                try
                {
                    var mapParamControl = MapParamControl;
                    if (!mapParamControl.ContainsKey(iparam))
                    {
                        //Remove old parameters from the last time first
                        //var oldParamsToRemove = mapParamControl.Where(x => x.Key.Name.Equals(iparam.Name))
                        //    .Select(x => x.Key).ToList();
                        //oldParamsToRemove.ForEach(x => mapParamControl.Remove(x));

                        //add new parameter in
                        var control = CreateControl(iparam);
                        control.Size = new Size(MControlWidth, control.Size.Height);
                        mapParamControl.Add(iparam, control);
                        //System.Diagnostics.Debug.WriteLine(string.Format("create control for = {0}", iparam.Name));
                    }
                    var result = mapParamControl[iparam];
                    return result;
                }
                catch (Exception e)
                {
                    EmcLog.Error(e.ToString());
                    MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }

            public override bool ValidateParameter(Component sender, CSMGridParameter iparam)
            {
                return true;
            }
        }
    }
}