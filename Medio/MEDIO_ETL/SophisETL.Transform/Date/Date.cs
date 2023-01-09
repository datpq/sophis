using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SophisETL.Common;

using SophisETL.Transform.Date.Xml;

namespace SophisETL.Transform.Date
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class Date : AbstractBasicTransformTemplate
    {
        private Settings _Settings { get; set; }

        public override void Init()
        {
            base.Init();
        }

        protected override Record Transform(Record record)
        {            
            // check the syntax of the incoming XML, if it makes sense or not
            foreach (Xml.Date date in _Settings.date)
            {
                // make sure filter field is known
                if (!record.Fields.ContainsKey(date.dateName))
                {
                   continue;
                }

                // Extract the values used to convert
                DateTime fieldValue = (DateTime)record.Fields[date.dateName];
                DateTypeEnum dateTypeEnum = date.dateType;
                string dateSeparator = date.separator;
                string dateTypeComplement = date.dateTypeComplement;

                // Replace the DateTime b a string with the given format
                try
                {
                    record.Fields[date.dateName] = ConvertDate(fieldValue, dateTypeEnum, dateSeparator, dateTypeComplement);
                }
                catch (System.Exception ex)
                {                    
                    ex.GetType(); // avoid warning            
                    break;
                }                

            }
                        

            return record;
        }

        private string ConvertDate(DateTime fieldValue, DateTypeEnum dateType, string separator, string dateTypeComplement)
        {
            string result = null;

            switch (dateType)
            {
                case DateTypeEnum.DateDDMMYYYY:
                    result = fieldValue.ToString("dd" + separator + "MM" + separator + "yyyy");
                    break;
                case DateTypeEnum.DateYYYYMMDD:
                    result = fieldValue.ToString("yyyy" + separator + "MM" + separator + "dd");
                    break;
                case DateTypeEnum.DateType:
                    result = fieldValue.ToString(dateTypeComplement);
                    break;
            }
            return result;
        }
    }
}
