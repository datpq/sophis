using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using sophis.reporting;

namespace Eff
{
    namespace ToolkitReporting.NET
    {
        public class TokenTranslatorLastDayOfMonth : CSMTokenTranslator
        {
            public Control Control { get; set; }
            private static TokenTranslatorLastDayOfMonth instance = new TokenTranslatorLastDayOfMonth();

            public static TokenTranslatorLastDayOfMonth GetInstance()
            {
                return instance;
            }

            public override string Translate()
            {
                var dateTimePicker = Control as DateTimePicker;
                string result = dateTimePicker == null ? string.Empty : new DateTime(dateTimePicker.Value.Year, dateTimePicker.Value.Month + 1, 1).ToShortDateString();
                System.Diagnostics.Debug.WriteLine(string.Format("result={0}", result));
                return result;
            }
        }
    }
}