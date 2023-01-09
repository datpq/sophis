using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Excel = Microsoft.Office.Interop.Excel;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Tools;
using SophisETL.Common.ErrorMgt;
using SophisETL.Queue;

using SophisETL.Extract.FIXExtract.Xml;


namespace SophisETL.Extract.FIXExtract
{

    // FIXExtract is a Pullable Extractor
    // i.e. records are pulled directly by the Queue
    [SettingsType(typeof(Settings), "_Settings")]
    public class FIXExtract : IExtractStep
    {
        //// Internal Memebers
        private FIXReader _FIXReader = new FIXReader();
        private Settings _Settings { get; set; }
        private string[] _Headers;
        private int _RecordsExtractedCount = 0;

        #region FIXExtract Chain Parameters
        //// Setup of the Chain including this extract

        public string Name { get; set; }

        // Only an Output Queue exists
        public IETLQueue TargetQueue { get; set; }

        #endregion

        public void Start()
        {
            LogManager.Instance.Log("Extract/CSV/" + Name + ": starting step");

            while (_FIXReader.Next())
            {
                // Should we consider this line as the header line
                if (_FIXReader.CurrentRecordIndex == _Settings.headerLine)
                    _Headers = _FIXReader.GetFields();

                // Should we skip this line from being sent as a record?
                if (_FIXReader.CurrentRecordIndex <= _Settings.skipLine)
                    continue;

                Record record = Record.NewRecord(_FIXReader.CurrentRecordIndex);

                for (int i = 0; i < Math.Max(_FIXReader.GetFieldCount(), MaxFieldPositionInDefinition); i++)
                {
                    try
                    {
                        // check Field Definition
                        FIXField fieldDefinition = GetDefinitionOfField(i);

                        // Get Read value
                        object readValue = null;

                        // Protect against nullable fields at the end of the line
                        if (i < _FIXReader.GetFieldCount())
                        {
                            if (fieldDefinition.xmlFieldType == FIXFieldTypeEnum.String)
                                readValue = _FIXReader.GetString(i);

                            if (fieldDefinition.xmlFieldType == FIXFieldTypeEnum.Number)
                                if (!fieldDefinition.nullable)
                                    readValue = _FIXReader.GetDouble(i);
                                else
                                    readValue = _FIXReader.GetNullableDouble(i);

                            if (fieldDefinition.xmlFieldType == FIXFieldTypeEnum.DateYYYYMMDD)
                                if (!fieldDefinition.nullable)
                                    readValue = _FIXReader.GetDate(i, "yyyy" + fieldDefinition.separator + "MM" + fieldDefinition.separator + "dd");
                                else
                                    readValue = _FIXReader.GetNullableDate(i, "yyyy" + fieldDefinition.separator + "MM " + fieldDefinition.separator + "dd");

                            else if (fieldDefinition.xmlFieldType == FIXFieldTypeEnum.DateDDMMYYYY)
                                if (!fieldDefinition.nullable)
                                    readValue = _FIXReader.GetDate(i, "dd" + fieldDefinition.separator + "MM" + fieldDefinition.separator + "yyyy");
                                else
                                    readValue = _FIXReader.GetNullableDate(i, "dd" + fieldDefinition.separator + "MM" + fieldDefinition.separator + "yyyy");
                        }
                        //LogManager.Instance.Log("readvalue" + readValue + "|");

                        // Autoboxing makes sure that our Nullable<> type has been properly converted to object
                        // (cf. http://msdn.microsoft.com/en-us/library/ms228597%28v=VS.90%29.aspx)
                        if (fieldDefinition.nullable)
                            // Only added for Nullable fields
                            record.Fields.Add(fieldDefinition.xmlFieldName + "_IsNull", (readValue == null));
                        else if (readValue == null || (readValue is string && ((string)readValue).Trim().Length == 0))
                            // We have a "null" non-nullable field!
                            throw new Exception("This field can not be null!");

                        record.Fields.Add("f"+fieldDefinition.xmlFieldName, readValue);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.Instance.HandleError(new ETLError
                        {
                            Exception = ex,
                            Step = this,
                            Record = record,
                            Message = "On record " + _FIXReader.CurrentRecordIndex + " failed to read properly field " + (i + 1) + " with value " + _FIXReader.GetString(i)
                        });
                        // skip it
                        record = null;
                        break;
                    }
                }

                TargetQueue.Enqueue(record);
                _RecordsExtractedCount++;
            }
         
            LogManager.Instance.Log("Extract/CSV/" + Name + ": step finished - "
                + _RecordsExtractedCount + " record(s) extracted");
        }

        private FIXField GetDefinitionOfField(int p)
        {
            if (_Settings.FIX != null)
            {
                foreach (FIXField field in _Settings.FIX)
                {
                    if (field.xmlFieldName == _FIXReader.GetPartialFields(p))
                    {
                        //LogManager.Instance.Log(field.xmlFieldName + ": retrieved - " + _FIXReader.GetPartialFields(p));
                        return field;
                    }
                }
            }
            // Not Found - send default field
            FIXField defaultField = new FIXField();
            defaultField.position = p;

            // Name can be deduced from the headers if we have read an Headers Line
            string headerName;
            if (_Headers != null && _Headers.Length >= p && (headerName = _Headers[p - 1].Trim()).Length > 0)
                defaultField.xmlFieldName = headerName;
            else
                defaultField.xmlFieldName = "field" + p.ToString();
            defaultField.xmlFieldName = defaultField.xmlFieldName.Replace(' ', '_'); // no blank space in field names

            defaultField.xmlFieldType = FIXFieldTypeEnum.String;
            defaultField.nullable = true;
            return defaultField;
        }

        // Return the largest field position defined in the input file (allows us to catch null fields at the end of the record)
        private int _MaxFieldPositionInDefinition = -1;
        private int MaxFieldPositionInDefinition
        {
            get
            {
                if (_MaxFieldPositionInDefinition == -1)
                {
                    if (_Settings.FIX != null)
                        _MaxFieldPositionInDefinition = (from f in _Settings.FIX select f.position).Max();
                    else
                        _MaxFieldPositionInDefinition = 0;
                }
                return _MaxFieldPositionInDefinition;
            }
        }

        //// Interface
        #region IETLStep Members

        public void Init()
        {
            // Transmit the relevant settings to the CSV Reader
            _FIXReader.InputFileName = _Settings.inputFile;

            string extension = Path.GetExtension(_FIXReader.InputFileName);
            string targetFile = "";
            string targetFileName = "";
            if (extension != ".csv")
            {
                Excel.Application app = new Excel.Application();
                app.DisplayAlerts = false;
                Excel.Workbook workbook = app.Workbooks.Open(Path.GetFullPath(_FIXReader.InputFileName));
                targetFile = _FIXReader.InputFileName.Substring(0, _FIXReader.InputFileName.Length - extension.Length) + ".csv";
                targetFileName = Path.GetFullPath(targetFile);
                workbook.SaveAs(targetFileName, Excel.XlFileFormat.xlCSV);
                workbook.Close();
                app.Quit();

                _FIXReader.InputFileName = targetFile;
            }

            if (_Settings.separator != null && _Settings.separator.Length > 0)
                _FIXReader.FieldSeparator = _Settings.separator[0];
            if (_Settings.escape != null && _Settings.escape.Length > 0)
                _FIXReader.FieldEscape = _Settings.escape[0];
            _FIXReader.SkipEmptyLines = true; // not configurable - hard-coded

            // We try to open the input file to see if we have any problem doing so
            try
            {
                FileStream stream = File.OpenRead(_FIXReader.InputFileName);
                stream.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error: can not open input CSV File for Reading", ex);
            }
        }

        #endregion
    }
}
