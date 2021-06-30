using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Eff.ToolkitReporting.NET.PropertyGrid;
using Eff.Utils;
using ToolkitReporting.NET;

// ReSharper disable once CheckNamespace
namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public class ExcelTemplateEditor : FileNameEditorWithParameters
        {
            public ExcelTemplateEditor() : base(Resource.ExcelTemplateFilter, Resource.ExcelTemplateTitle) {}

            public static Attribute CreateEditorAttribute()
            {
                return new EditorAttribute(typeof(ExcelTemplateEditor), typeof(UITypeEditor));
            }
        }

        public class ExcelOutputFileEditor : FileNameEditorWithParameters
        {
            public ExcelOutputFileEditor() : base(Resource.ExcelTemplateFilter, Resource.ExcelTemplateTitle) {}

            public static Attribute CreateEditorAttribute()
            {
                return new EditorAttribute(typeof(ExcelOutputFileEditor), typeof(UITypeEditor));
            }
        }

        public class FileNameEditorWithParameters : FileNameEditor
        {
            private readonly string _dialogFilter;
            private readonly string _dialogTitle;

            public FileNameEditorWithParameters(string dialogFilter, string dialogTitle)
            {
                _dialogFilter = dialogFilter;
                _dialogTitle = dialogTitle;
            }

            protected override void InitializeDialog(OpenFileDialog openFileDialog)
            {
                openFileDialog.Filter = _dialogFilter;
                openFileDialog.Title = _dialogTitle;
                openFileDialog.CheckFileExists = false;
            }
        }

        public class GridComboBox : UITypeEditor
        {
            private readonly ListBox _listBox;
            private bool _escKeyPressed;
            private IWindowsFormsEditorService _editorService;

            public GridComboBox()
            {
                _listBox = new ListBox();

                // Properties
                _listBox.BorderStyle = BorderStyle.None;

                // Events
                _listBox.Click += (sender, args) =>
                {
                    if (_editorService != null)
                    {
                        _editorService.CloseDropDown();
                    }
                };

                _listBox.PreviewKeyDown += (sender, args) =>
                {
                    if (args.KeyCode == Keys.Escape)
                        _escKeyPressed = true;
                };
            }

            private void PopulateListBox(ITypeDescriptorContext context, object currentValue)
            {
                try
                {
                    // Clear List
                    _listBox.Items.Clear();
                    var reportingObject = context.Instance as ReportingObject;
                    if (reportingObject != null && context.PropertyDescriptor != null)
                    {
                        if (context.PropertyDescriptor.Name.EndsWith(ReportingObject.SettingWorksheet))
                            // ReSharper disable once CoVariantArrayConversion
                            _listBox.Items.AddRange(reportingObject.GetWorkSheets());
                        else if (context.PropertyDescriptor.Name.EndsWith(ReportingObject.SettingTableTag))
                            // ReSharper disable once CoVariantArrayConversion
                            _listBox.Items.AddRange(reportingObject.GetTableTagsBySource(context.PropertyDescriptor.Name.Split('.')[0]));
                    }

                    // Select current item 
                    if (currentValue != null)
                        _listBox.SelectedItem = currentValue;

                    // Set the height based on the Items in the ListBox
                    _listBox.Height = _listBox.PreferredHeight;
                }
                catch (Exception e)
                {
                    EmcLog.Error(e.ToString());
                    MessageBox.Show(string.Format("Error: {0}", e.Message), MainClass.Caption, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                if ((context != null) && (provider != null))
                {
                    //Uses the IWindowsFormsEditorService to display a 
                    // drop-down UI in the Properties window:
                    _editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

                    if (_editorService != null)
                    {
                        // Add Values to the ListBox
                        PopulateListBox(context, value);

                        // Set to false before showing the control
                        _escKeyPressed = false;

                        // Attach the ListBox to the DropDown Control
                        _editorService.DropDownControl(_listBox);

                        // User pressed the ESC key --> Return the Old Value
                        if (!_escKeyPressed)
                        {
                            // Get the Selected Object
                            object obj = _listBox.SelectedItem;

                            // If an Object is Selected --> Return it
                            if (obj != null)
                                return (obj);
                        }
                    }
                }

                return value;
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return (UITypeEditorEditStyle.DropDown);
            }

            public static Attribute CreateEditorAttribute()
            {
                return new EditorAttribute(typeof(GridComboBox), typeof(UITypeEditor));
            }
        }

        public class PortfolioSourceParamColMap : UITypeEditor
        {
            private IWindowsFormsEditorService _editorService;
            private ParamColMapForm _paramColMapForm;

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return (UITypeEditorEditStyle.Modal);
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                if (context != null && provider != null)
                {
                    //Uses the IWindowsFormsEditorService to display a 
                    // drop-down UI in the Properties window:
                    _editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                    var reportingObject = context.Instance as ReportingObject;
                    if (_editorService != null && reportingObject != null && context.PropertyDescriptor != null)
                    {
                        if (null == _paramColMapForm)
                        {
                            //_paramColMapForm = new ParamColMapForm(reportingObject, context.PropertyDescriptor.Name);
                        }
                        //_paramColMapForm.Initialize((string)value);
                        switch (_editorService.ShowDialog(_paramColMapForm))
                        {
                            case DialogResult.OK:
                                //value = _paramColMapForm.MapValue;
                                reportingObject.SetMember(context.PropertyDescriptor.Name, value);
                                break;
                            case DialogResult.Cancel:
                                break;
                        }
                    }
                }

                return value;
            }

            public static Attribute CreateEditorAttribute()
            {
                return new EditorAttribute(typeof(PortfolioSourceParamColMap), typeof(UITypeEditor));
            }
        }
    }
}