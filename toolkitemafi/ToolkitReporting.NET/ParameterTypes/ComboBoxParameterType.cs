using System.Windows.Forms;
using Oracle.DataAccess.Client;
using Sophis.Reporting.Controls.CommonTypes;

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public class ComboBoxParameterType : AbstractParameterType
        {
            private const int ControlWidth = 260;
            private readonly string _mQuery;

            public ComboBoxParameterType(string query) : base(ControlWidth)
            {
                _mQuery = query;
            }

            protected override Control CreateControl(CSMGridParameter iparam)
            {
                var comboBoxControl = new ComboBox {DropDownStyle = ComboBoxStyle.DropDownList};
                comboBoxControl.SelectedIndexChanged += (sender, args) =>
                {
                    var selectedItem = (ComboboxItem)((ComboBox)sender).SelectedItem;
                    iparam.Value = selectedItem.Value.ToString();
                };

                var connection = Sophis.DataAccess.DBContext.Connection;
                using (var command = new OracleCommand(_mQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new ComboboxItem
                            {
                                Value = reader.GetValue(0),
                                Text = (reader.FieldCount == 1 ? reader.GetValue(0) : reader.GetValue(1)).ToString()
                            };
                            comboBoxControl.Items.Add(item);
                        }

                        reader.Close();
                    }
                }

                return comboBoxControl;
            }
        }

        public class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}
