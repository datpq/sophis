using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SophisETL.Common;
using SophisETL.Common.Logger;

using SophisETL.Transform.ComputeOffsetNAV.Xml;

namespace SophisETL.Transform.ComputeOffsetNAV
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class ComputeOffsetNAV : AbstractBasicTransformTemplate
    {
        private Settings _Settings { get; set; }

        public override void Init()
        {
            base.Init();
        }

        protected override Record Transform(Record record)
        {            
            // check the syntax of the incoming XML, if it makes sense or not
            foreach (Xml.ComputeElement computeVal in _Settings.ComputeList)
            {
                // make sure filter field is known
                LogManager.Instance.Log("Ensuring that the target field ''" + computeVal.computeField + "'' , " + " exists");
                if (!record.Fields.ContainsKey(computeVal.computeField))
                {
                    LogManager.Instance.Log("Field ''" + computeVal.computeField + "'' , " + " does NOT exist!!");
                    continue;
                }
                else { LogManager.Instance.Log("Field ''" + computeVal.computeField + "'' , " + " exists"); }

                // Extract the values used to convert

                double fieldValue = (double)record.Fields[computeVal.computeField];
                LogManager.Instance.Log("Current value in field ''" + computeVal.computeField + "'' , " + " is: " + fieldValue.ToString() + ";");

                // Replace the DateTime b a string with the given format
                try
                {
                    if (fieldValue != 0.00) 
                    { 
                        record.Fields[computeVal.computeField] = "2468.13579";
                        LogManager.Instance.Log("Record in field ''" + computeVal.computeField + "'' , " + " updated");
                    }
                    
                    //record.Fields[date.dateName] = ConvertDate(fieldValue, dateTypeEnum, dateSeparator, dateTypeComplement);
                }
                catch (System.Exception ex)
                {                    
                    ex.GetType(); // avoid warning            
                    break;
                }                

            }
                        

            return record;
        }
        /*
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
         */
    }
}
