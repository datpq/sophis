using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using RichMarketAdapter.ticket;
using sophis.backoffice_kernel;
using sophis.log;
using sophis.utils;
using transaction;

namespace Mediolanum_RMA_FILTER.Tools
{
    public static class CSxUtil
    {
        private static string _ClassName = typeof(CSxUtil).Name;
        private const string DefaultDateFormat = "dMMyyyy";
        private static CultureInfo DefaultCultureInfo = new CultureInfo("en-US");
        private static readonly DateTime refdate = new DateTime(1904, 01, 01, 0, 0, 0);

        public static int ToInt32(this string str)
        {
            int res = 0;
            res = Int32.TryParse(str, out res) ? res : 0;
            return res;
        }

        public static double ToDouble(this string str)
        {
            double res = 0;
            res = Double.TryParse(str, out res) ? res: 0.0 ;
            return res;
        }

        public static string ToSafeString(this object obj)
        {
            return (obj ?? string.Empty).ToString();
        }

        public static string GetLast8Characters(this string str)
        {
            if (str.ValidateNotEmpty())
            {
                if (str.Length >= 8)
                    str = str.Substring((str.Length - 8), 8);
            }
            return str;
        }

        /// <summary>
        /// Split string into a list of strings 
        /// </summary>
        /// <param name="csv"></param>
        /// <param name="separator"></param>
        /// <param name="removeDoubleQuotes"></param>
        /// <param name="removeSingleQuotes"></param>
        /// <returns></returns>
        public static List<string> SplitCSV(this string csv, char separator = ';', bool removeDoubleQuotes = true, bool removeSingleQuotes = true)
        {
            if (!String.IsNullOrEmpty(csv))
            {
                List<string> fields = csv.Split(separator).ToList();
                for (int i = 0; i < fields.Count; i++)
                {
                    fields[i] = removeDoubleQuotes ? fields[i].Replace(@"""", "") : fields[i];
                    fields[i] = removeSingleQuotes ? fields[i].Replace(@"''", "") : fields[i];
                    fields[i] = fields[i].Trim();
                }
                return fields;
            }
            else
                return new List<string>();
        }

        public static int ToSophisDate(this DateTime date)
        {

            TimeSpan diff = date - refdate.Date;
            return Convert.ToInt32(diff.TotalDays);
        }

        /// <summary>
        /// Return sophis date by string date
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static int GetDateInFormat(string input, string format = DefaultDateFormat)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int res = 0;
                try
                {
                    DateTime dateTime;
                    DateTime.TryParseExact(input, format, DefaultCultureInfo, DateTimeStyles.AssumeLocal, out dateTime);
                    res = dateTime.ToSophisDate();
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Failed to convert date format. Exception: " + ex.Message + ". Stack trace: " + ex.StackTrace);
                }
                return res;
            }
        }

        /// <summary>
        /// Access value in RBC files  
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetValue(this List<string> list, int index)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                string res = "";
                if (list != null)
                {
                    try
                    {
                        if (index < list.Count)
                        {
                            res = list.ElementAtOrDefault(index) ?? list[index];
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Exception occurred while trying to access list element at index = " + index + ". Message: " + ex.Message);
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// If string is a valid date given a default date format
        /// </summary>
        /// <param name="date"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool ValidateDate(this string date, string format = DefaultDateFormat)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    DateTime.ParseExact(date, format, DefaultCultureInfo);
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Exception " + ex.Message);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Return sophis date by string date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="allowedFormats"></param>
        /// <returns></returns>
        public static int GetDateInAnyFormat(this string date, params string[] allowedFormats)
        {
            int retval = 0;
            string dateFormat = GetDateFormat(date, allowedFormats);
            if (!String.IsNullOrEmpty(dateFormat))
            {
                retval = GetDateInFormat(date, dateFormat);
            }
            return retval;
        }

        /// <summary>
        /// If string is a valid date given date formats
        /// </summary>
        /// <param name="date"></param>
        /// <param name="allowedFormats"></param>
        /// <returns></returns>
        private static string GetDateFormat(string date, params string[] allowedFormats)
        {
            string retval = "";
            if (allowedFormats != null)
            {
                for (int i = 0; i < allowedFormats.Length; i++)
                {
                    bool success = true;
                    try
                    {
                        DateTime.ParseExact(date, allowedFormats[i], DefaultCultureInfo);
                    }
                    catch (Exception ex)
                    {
                        success = false;
                    }
                    if (success)
                    {
                        retval = allowedFormats[i];
                        break;
                    }
                }
            }
            return retval;
        }

        /// <summary>
        /// If string is a valid date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="allowedFormats"></param>
        /// <returns></returns>
        public static bool ValidateDate(this string date, params string[] allowedFormats)
        {
            bool retval = false;
            string dateFormat = GetDateFormat(date, allowedFormats);
            if (!String.IsNullOrEmpty(dateFormat))
            {
                retval = true;
            }
            return retval;
        }

        /// <summary>
        /// If string value is a currency 
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static bool ValidateCurrency(this string currency)
        {
            if (!String.IsNullOrEmpty(currency))
            {
                if (!CSxRBCHelper.GetAllCurrencies().Contains(currency.ToUpper())) //case insensitive validation
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// If string can be cast as Int64
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool ValidateInteger(this string number)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    Int64.Parse(number);
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Exception " + ex.Message);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// If string can be cast as Double
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool ValidateDouble(this string number)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    Double.Parse(number);
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Exception " + ex.Message);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// If string is not null nor empty
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool ValidateNotEmpty(this string str)
        {
            return !String.IsNullOrEmpty(str);
        }

        public static void SetUserField(this ITicketMessage ticketMessage, string fieldName, string value)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    ticketMessage.addUserField(fieldName, value);
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Exception while seting user fields : " + ex.Message);
                }
            }
        }

        public static void SetTicketField<T>(this ITicketMessage ticketMessage, FieldId fieldId, T value, bool reverseValue = false)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    if (reverseValue)
                    {
                        int intValue = 0;
                        double doubleValue = 0.0;
                        long longValue = 0;
                        if (Int32.TryParse(value.ToString(), out intValue))
                        {
                            ticketMessage.add(fieldId, -intValue);
                            logger.log(Severity.info, "Successfully added (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")) value = " + -intValue + " to message. Value is reversed! Orginal value = " + value);
                        }
                        else if (Double.TryParse(value.ToString(), out doubleValue))
                        {
                            ticketMessage.add(fieldId, -doubleValue);
                            logger.log(Severity.info, "Successfully added (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")) value = " + -doubleValue + " to message. Value is reversed! Orginal value = " + value);
                        }
                        else if (long.TryParse(value.ToString(), out longValue))
                        {
                            ticketMessage.add(fieldId, -longValue);
                            logger.log(Severity.info, "Successfully added (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")) value = " + -longValue + " to message. Value is reversed! Orginal value = " + value);
                        }
                    }
                    else
                    {
                        ticketMessage.add(fieldId, value);
                        logger.log(Severity.info, "Successfully added (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")) value = " + value + " to message");
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Failed to set value = " + value + " to message (FieldId = " + Enum.GetName(typeof(FieldId), fieldId) + " (" + fieldId + ")). Exception: " + ex.Message + ". Stack trace: " + ex.StackTrace);
                }
            }
        }
    }

}
