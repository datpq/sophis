using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SophisETL.Common;
using SophisETL.Transform.TickerToReplaceISIN.Xml;

namespace SophisETL.Transform.TickerToReplaceISIN
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class TickerToReplaceISIN : AbstractBasicTransformTemplate
    {
        //// Internal Members
        private Settings _Settings { get; set; }

        protected override Record Transform(Record record)
        {
            foreach (Xml.Replace replace in _Settings.replace)
            {
                string refSName = (string)record.Fields[replace.referenceName].ToString();
                if (refSName == replace.ReferenceMatch)
                {
                    string source = (string)record.Fields[replace.TICKERField].ToString();
                    record.SafeAdd(replace.ISINField, source, eExistsAction.replace);
                }
            }
            return record;
        }

    }
}
