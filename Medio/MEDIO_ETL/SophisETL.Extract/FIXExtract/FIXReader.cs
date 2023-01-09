using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;

using SophisETL.Common.Logger;

namespace SophisETL.Extract.FIXExtract
{
    /// <summary>
    /// Generic CSV file reader (interfaced as a OracleDataReader)
    /// 1. Set the File Name
    /// 2. Set the Separator if necessary (default is ;)
    /// 3. call next() and Get{FieldType}(fieldIndex) as long as
    ///    next() returns true
    /// 
    /// History:
    ///    v1.0 (20080616,AdB) initial version
    ///    v2.0 (20081111,AdB) added support for quotes enclosed fields
    ///    v3.0 (20110110,AdB) added support for null fields and nullable types
    /// </summary>
    class FIXReader
    {
        private String[] fCurrentRecordFields;

        /// <summary>
        /// Input File Name (as a String)
        /// Changing it will reset the input stream
        /// </summary>
        public string InputFileName
        { get { return fInputFileName; } set { fInputFileName = value; InputStreamReader = null; } }
        private string fInputFileName;


        /// <summary>
        /// Field Separator (default value is a comma: , )
        /// </summary>
        public char FieldSeparator
        { get { return fFieldSeparator; } set { fFieldSeparator = value; } }
        private char fFieldSeparator = ',';

        /// <summary>
        /// Field Escape (default value is a double quote: " )
        /// </summary>
        public char FieldEscape
        { get { return fFieldEscape; } set { fFieldEscape = value; } }
        private char fFieldEscape = '"';

        /// <summary>
        /// Set to true to skip empty lines (record index will still be incremented on
        /// blank lines but Next() will skip them)
        /// </summary>
        public bool SkipEmptyLines { get; set; }

        /// <summary>
        /// Index of the current record
        /// 0 means no record has been loaded yet
        /// 1 is the first record index
        /// </summary>
        public int CurrentRecordIndex
        { get { return fCurrentRecordIndex; } }
        private int fCurrentRecordIndex;



        //// Internal Stream manipulation ////
        private StreamReader fInputStreamReader = null;
        private StreamReader InputStreamReader
        {
            get
            {
                // if null, reopen and resets indicators
                if (fInputStreamReader == null)
                {
                    fInputStreamReader = new StreamReader(
                        new FileStream(InputFileName, FileMode.Open, FileAccess.Read), Encoding.UTF8);
                    fCurrentRecordIndex = 0;
                }
                return fInputStreamReader;
            }

            set
            {
                // if not null, close and dispose
                if (fInputStreamReader != null)
                    fInputStreamReader.Close();
                fInputStreamReader = value;
            }
        }


        /// <summary>
        /// Load the next record in the buffer
        /// </summary>
        /// <returns>false if there is no more record to load</returns>
        public bool Next()
        {
            string nextLine = "";
            bool nextLineFound = true;
            do
            {
                if (InputStreamReader.EndOfStream)
                    return false;
                // Read the Next Line
                nextLine = InputStreamReader.ReadLine();
                fCurrentRecordIndex += 1;
                // In skip empty lines mode, we must check this
                nextLineFound = (!SkipEmptyLines) || (nextLine.Trim().Length > 0);

            } while (!nextLineFound);


            // Cut it into fields
            fCurrentRecordFields = SplitFields(nextLine);

            return true;
        }


        /// <summary>
        /// Cut a raw record into separate string fields.
        /// Fields are separated by the standard Separator (usually , or ;) but the
        /// separators can be escaped, for example if included in a "..." quote enclosed
        /// field.
        /// Example:  a,b,"c,d,e",f  is only 4 fields: |a| |b| |c,d,e| and |f|
        /// 
        /// </summary>
        /// <param name="record">raw record on a single line</param>
        /// <returns>an array of strings with the splitted up fields</returns>
        private string[] SplitFields(string record)
        {
            string[] allSplit = record.Split(new char[] { fFieldSeparator });

            // Fast Path if no Escape character is detected (simple case)
            if (record.IndexOf(fFieldEscape) == -1)
                return allSplit;

            // Field concatenation if some Escape characters existed
            int fieldRead, fieldWrite;
            bool concatMode;
            string concatString;
            for (fieldRead = 0, fieldWrite = 0, concatMode = false, concatString = "";
                 fieldRead < allSplit.Length; fieldRead++)
            {
                if (allSplit[fieldRead].StartsWith("" + fFieldEscape))
                    concatMode = true;

                if (concatMode)
                    concatString += allSplit[fieldRead] + fFieldSeparator;
                else
                    allSplit[fieldWrite++] = allSplit[fieldRead];

                // Stop concat mode if a closing " is found, or if we reached the last field
                // of the record (case of missing closing ")
                if (concatMode
                    && (allSplit[fieldRead].EndsWith("" + fFieldEscape)
                       || fieldRead == allSplit.Length - 1))
                {
                    // Close concat field: remove beginning and trailing ", from field
                    int nToCut = (fieldRead == allSplit.Length - 1) ? 2 : 3;
                    allSplit[fieldWrite++] = concatString.Substring(1, concatString.Length - nToCut);
                    concatMode = false;
                    concatString = "";
                }
            }

            // Truncate the record to size fieldWrite if some fields have been concatenated
            string[] result = allSplit;
            if (fieldWrite != fieldRead)
            {
                result = new string[fieldWrite];
                Array.Copy(allSplit, result, fieldWrite);
            }

            return result;
        }

        /// <summary>
        /// Number of available fields in the current Record
        /// </summary>
        /// <returns>the number of fields as an integer</returns>
        public int GetFieldCount()
        {
            if (fCurrentRecordFields != null)
                return fCurrentRecordFields.Length;
            else
                return 0;
        }


        /// <summary>
        /// When access to the entire record is necessary
        /// </summary>
        /// <returns></returns>
        public string[] GetFields()
        {
            return fCurrentRecordFields;
        }

        public string GetPartialFields(int field)
        {
            string temp = fCurrentRecordFields[field];
            int index = temp.IndexOf("=");
            if(index > 0) temp = temp.Substring(0, temp.IndexOf("="));
            return Convert.ToString(temp);
        }

        /// <summary>
        /// Returns true if the "field" field is Null (empty or not defined)
        /// </summary>
        /// <param name="field">Field index, starting with 0</param>
        public bool IsNullField(int field)
        {
            // Null if out of the current size
            if (field >= fCurrentRecordFields.Length)
                return true;
            string fieldStr = fCurrentRecordFields[field];
            return (string.IsNullOrEmpty(fieldStr) || fieldStr.Trim().Length == 0);
        }

        /// <summary>
        /// Returns the "field" field as a String
        /// </summary>
        /// <param name="field">Field index, starting with 0</param>
        public string GetString(int field)
        {
            string temp = fCurrentRecordFields[field];
            temp = temp.Substring(temp.IndexOf("=") + 1);
            //LogManager.Instance.Log("Extract/CSV/FieldString " + temp);
            return Convert.ToString(temp);
        }


        /// <summary>
        /// Returns the "field" field as an Integer
        /// </summary>
        /// <param name="field">Field index, starting with 0</param>
        public int GetInt(int field)
        {
            fCurrentRecordFields[field] = fCurrentRecordFields[field].Substring(fCurrentRecordFields[field].IndexOf("=") + 1);
            return Convert.ToInt32(fCurrentRecordFields[field]);
        }


        /// <summary>
        /// Returns the "field" field as a Double
        /// </summary>
        /// <param name="field">Field index, starting with 0</param>
        public double GetDouble(int field)
        {
            fCurrentRecordFields[field] = fCurrentRecordFields[field].Substring(fCurrentRecordFields[field].IndexOf("=") + 1);
            return XmlConvert.ToDouble(fCurrentRecordFields[field]);
        }
        public double? GetNullableDouble(int field)
        {
            double? result = null;
            if (!IsNullField(field))
                result = GetDouble(field);

            return result;
        }

        /// <summary>
        /// Returns the "field" field as a Double with a specified formatter
        /// </summary>
        /// <param name="field">Field index, starting with 0</param>
        /// <param name="format">Locale to use for conversion</param>
        public double GetDouble(int field, IFormatProvider format)
        {
            fCurrentRecordFields[field] = fCurrentRecordFields[field].Substring(fCurrentRecordFields[field].IndexOf("=") + 1);
            return Convert.ToDouble(fCurrentRecordFields[field], format);
        }


        /// <summary>
        /// Returns the "field" field as a Date
        /// </summary>
        /// <param name="field">Field index, starting with 0</param>
        public DateTime GetDate(int field)
        {
            fCurrentRecordFields[field] = fCurrentRecordFields[field].Substring(fCurrentRecordFields[field].IndexOf("=") + 1);
            return DateTime.Parse(fCurrentRecordFields[field]);
        }


        /// <summary>
        /// Returns the "field" field as a Date, using the specified format
        /// </summary>
        /// <param name="field">Field index, starting with 0</param>
        /// <param name="format">Date format</param>
        public DateTime GetDate(int field, string format)
        {
            fCurrentRecordFields[field] = fCurrentRecordFields[field].Substring(fCurrentRecordFields[field].IndexOf("=") + 1);
            return DateTime.ParseExact(fCurrentRecordFields[field].Trim(), format,
                System.Globalization.CultureInfo.CurrentCulture);
        }
        public DateTime? GetNullableDate(int field, string format)
        {
            DateTime? result = null;
            if (!IsNullField(field))
                result = GetDate(field, format);

            return result;
        }

        /// <summary>
        /// Close the underlying stream and dispose of all the used resources
        /// </summary>
        public void Close()
        {
            fCurrentRecordFields = new String[] { };
            InputStreamReader = null; // will close and dispose
        }
    }
}
