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

using SophisETL.Extract.CSVExtract.Xml;
using System.Diagnostics;


namespace SophisETL.Extract.CSVExtract
{

    // CSVExtract is a Pullable Extractor
    // i.e. records are pulled directly by the Queue
    [SettingsType(typeof(Settings), "_Settings")]
    public class CSVExtract : IExtractStep
    {
        //// Internal Memebers
        private CSxCSVReader _CSVReader = new CSxCSVReader();
        private Settings _Settings { get; set; }
        private string[] _Headers;
        private int _RecordsExtractedCount = 0;

        #region CSVExtract Chain Parameters
        //// Setup of the Chain including this extract

        public string Name { get; set; }

        // Only an Output Queue exists
        public IETLQueue TargetQueue { get; set; }

        #endregion
        
        public void Start()
        {

            LogManager.Instance.Log("Extract/CSV/" + Name + ": starting step");

            while (_CSVReader.Next())
            {
                // Should we consider this line as the header line
                if (_CSVReader.CurrentRecordIndex == _Settings.headerLine)
                    _Headers = _CSVReader.GetFields();

                // Should we skip this line from being sent as a record?
                if (_CSVReader.CurrentRecordIndex <= _Settings.skipLine)
                    continue;

                Record record = Record.NewRecord(_CSVReader.CurrentRecordIndex);

                for (int i = 0; i < Math.Max(_CSVReader.GetFieldCount(), MaxFieldPositionInDefinition); i++)
                {
                    try
                    {
                        // check Field Definition
                        CsvField fieldDefinition = GetDefinitionOfField(i + 1);

                        // Get Read value
                        object readValue = null;

                        // Protect against nullable fields at the end of the line
                        if (i < _CSVReader.GetFieldCount())
                        {
                            if (fieldDefinition.xmlFieldType == CsvFieldTypeEnum.String)
                                readValue = _CSVReader.GetString(i);

                            if (fieldDefinition.xmlFieldType == CsvFieldTypeEnum.Number)
                                if (!fieldDefinition.nullable)
                                    readValue = _CSVReader.GetDouble(i);
                                else
                                    readValue = _CSVReader.GetNullableDouble(i);

                            if (fieldDefinition.xmlFieldType == CsvFieldTypeEnum.DateYYYYMMDD)
                                if (!fieldDefinition.nullable)
                                    readValue = _CSVReader.GetDate(i, "yyyy" + fieldDefinition.separator + "MM" + fieldDefinition.separator + "dd");
                                else
                                    readValue = _CSVReader.GetNullableDate(i, "yyyy" + fieldDefinition.separator + "MM " + fieldDefinition.separator + "dd");

                            else if (fieldDefinition.xmlFieldType == CsvFieldTypeEnum.DateDDMMYYYY)
                                if (!fieldDefinition.nullable)
                                    readValue = _CSVReader.GetDate(i, "dd" + fieldDefinition.separator + "MM" + fieldDefinition.separator + "yyyy");
                                else
                                    readValue = _CSVReader.GetNullableDate(i, "dd" + fieldDefinition.separator + "MM" + fieldDefinition.separator + "yyyy");

                            else if (fieldDefinition.xmlFieldType == CsvFieldTypeEnum.DateDDMMMYYYY)
                                if (!fieldDefinition.nullable)
                                    readValue = _CSVReader.GetDate(i, "dd" + fieldDefinition.separator + "MMM" + fieldDefinition.separator + "yyyy");
                                else
                                    readValue = _CSVReader.GetNullableDate(i, "dd" + fieldDefinition.separator + "MMM" + fieldDefinition.separator + "yyyy");

                            else if (fieldDefinition.xmlFieldType == CsvFieldTypeEnum.DateType)
                                if (!fieldDefinition.nullable)
                                    readValue = _CSVReader.GetDate(i, fieldDefinition.xmlFieldTypeComplement);
                                else
                                    readValue = _CSVReader.GetNullableDate(i, fieldDefinition.xmlFieldTypeComplement);
                            else if (fieldDefinition.xmlFieldType == CsvFieldTypeEnum.TimeDouble)
                                readValue = _CSVReader.GetTimeSpan(i);
                        }

                        // Autoboxing makes sure that our Nullable<> type has been properly converted to object
                        // (cf. http://msdn.microsoft.com/en-us/library/ms228597%28v=VS.90%29.aspx)
                        if (fieldDefinition.nullable)
                            // Only added for Nullable fields
                            record.Fields.Add(fieldDefinition.xmlFieldName + "_IsNull", (readValue == null));
                        else if (readValue == null || (readValue is string && ((string)readValue).Trim().Length == 0))
                            // We have a "null" non-nullable field!
                            throw new Exception("This field can not be null!");

                        record.Fields.Add(fieldDefinition.xmlFieldName, readValue);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.Instance.HandleError(new ETLError
                        {
                            Exception = ex,
                            Step = this,
                            Record = record,
                            Message = "On record " + _CSVReader.CurrentRecordIndex + " failed to read properly field " + (i + 1) + " with value " + _CSVReader.GetString(i)
                        });
                        // skip it
                        record = null;
                        break;
                    }
                }

                TargetQueue.Enqueue(record);
                _RecordsExtractedCount++;
            }

            _CSVReader.Close();
            LogManager.Instance.Log("Extract/CSV/" + Name + ": step finished - "
                + _RecordsExtractedCount + " record(s) extracted");
        }

        private CsvField GetDefinitionOfField(int p)
        {

            if (_Settings.csvFields != null)
            {
                foreach (CsvField field in _Settings.csvFields)
                {
                    if (field.position == p)
                    {
                        return field;
                    }
                }
            }
            // Not Found - send default field
            CsvField defaultField = new CsvField();
            defaultField.position = p;

            // Name can be deduced from the headers if we have read an Headers Line
            string headerName;
            if (_Headers != null && _Headers.Length >= p && (headerName = _Headers[p - 1].Trim()).Length > 0)
                defaultField.xmlFieldName = headerName;
            else
                defaultField.xmlFieldName = "field" + p.ToString();
            defaultField.xmlFieldName = defaultField.xmlFieldName.Replace(' ', '_'); // no blank space in field names

            defaultField.xmlFieldType = CsvFieldTypeEnum.String;
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
                    if (_Settings.csvFields != null)
                        _MaxFieldPositionInDefinition = (from f in _Settings.csvFields select f.position).Max();
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
            //#if DEBUG
            //            Debugger.Launch();
            //#endif
            // Transmit the relevant settings to the CSV Reader
            _CSVReader.InputFileName = _Settings.inputFile;

            string extension = Path.GetExtension(_CSVReader.InputFileName);
            string targetFile = "";
            string targetFileName = "";
            if (String.Compare(extension, ".csv", true) != 0)
            {
                Excel.Application app = new Excel.Application();
                app.DisplayAlerts = false;
                Excel.Workbook workbook = app.Workbooks.Open(Path.GetFullPath(_CSVReader.InputFileName));
                targetFile = _CSVReader.InputFileName.Substring(0, _CSVReader.InputFileName.Length - extension.Length) + ".csv";
                targetFileName = Path.GetFullPath(targetFile);
                workbook.SaveAs(targetFileName, Excel.XlFileFormat.xlCSV);
                workbook.Close();
                app.Quit();

                if (File.Exists(_CSVReader.InputFileName))
                    File.Delete(_CSVReader.InputFileName);

                _CSVReader.InputFileName = targetFileName;
            }

            if (_Settings.separator != null && _Settings.separator.Length > 0)
                _CSVReader.FieldSeparator = _Settings.separator[0];
            if (_Settings.escape != null && _Settings.escape.Length > 0)
                _CSVReader.FieldEscape = _Settings.escape[0];
            _CSVReader.SkipEmptyLines = true; // not configurable - hard-coded

            // We try to open the input file to see if we have any problem doing so
            try
            {
                FileStream stream = File.OpenRead(_CSVReader.InputFileName);
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
