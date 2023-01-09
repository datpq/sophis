using System;
using System.Collections.Generic;
using System.IO;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Tools;
using SophisETL.Common.ErrorMgt;
using SophisETL.Queue;

using SophisETL.Load.CSVLoader.Xml;


namespace SophisETL.Load.CSVLoader
{


//     public class CSVExtract : IExtractStep
//     {
//         //// Internal Memebers
//         private CSxCSVReader _CSVReader = new CSxCSVReader();
//         private Settings     _Settings {get; set;}
//         private string[]     _Headers;
//         private int          _RecordsExtractedCount = 0;
// 
// 
//         #region CSVExtract Chain Parameters
//         //// Setup of the Chain including this extract
// 
//         public string Name { get; set; }
// 
//         // Only an Output Queue exists
//         public IETLQueue TargetQueue { get; set; }
// 
//         #endregion
// 
// 
//         // Start of our thread, we load and push
//         public event EventHandler NoMoreRecords;
// 
//         public void Start()
//         {
//             LogManager.Instance.Log( "Extract/CSV/" + Name + ": starting step" );
// 
//             while ( _CSVReader.Next() )
//             {
//                 // Should we consider this line as the header line
//                 if ( _CSVReader.CurrentRecordIndex == _Settings.headerLine )
//                     _Headers = _CSVReader.GetFields();
// 
//                 // Should we skip this line from being sent as a record?
//                 if ( _CSVReader.CurrentRecordIndex <= _Settings.skipLine )
//                     continue;
// 
//                 Record record = Record.NewRecord( _CSVReader.CurrentRecordIndex );
// 
//                 for ( int i = 0; i < _CSVReader.GetFieldCount(); i++ )
//                 {
//                     try
//                     {
//                         // check Field Definition
//                         CsvField fieldDefinition = GetDefinitionOfField( i + 1 );
//                         if ( fieldDefinition.xmlFieldType == CsvFieldTypeEnum.String )
//                             record.Fields.Add( fieldDefinition.xmlFieldName, _CSVReader.GetString( i ) );
// 
//                         if ( fieldDefinition.xmlFieldType == CsvFieldTypeEnum.Number )
//                             record.Fields.Add( fieldDefinition.xmlFieldName, _CSVReader.GetDouble( i ) );
// 
//                         if ( fieldDefinition.xmlFieldType == CsvFieldTypeEnum.DateYYYYMMDD )
//                             record.Fields.Add( fieldDefinition.xmlFieldName, _CSVReader.GetDate( i, "yyyyMMdd" ) );
//                     }
//                     catch ( Exception ex )
//                     {
//                         ErrorHandler.Instance.HandleError( new ETLError {
//                             Exception = ex,
//                             Step = this,
//                             Message = "On record " + _CSVReader.CurrentRecordIndex + " failed to read properly field " + i + " with value " + _CSVReader.GetString( i )
//                         } );
//                         // skip it
//                         record = null;
//                         break;
//                     }
//                 }
// 
//                 TargetQueue.Enqueue( record );
//                 _RecordsExtractedCount++;
//             }
//             // Notify the listeners that we are done producing records
//             NoMoreRecords( this, null );
// 
//             LogManager.Instance.Log( "Extract/CSV/" + Name + ": step finished - "
//                 + _RecordsExtractedCount + " record(s) extracted" );
//         }
// 
//         private CsvField GetDefinitionOfField( int p )
//         {
// 
//             if ( _Settings.csvFields != null )
//             {
//                 foreach ( CsvField field in _Settings.csvFields )
//                 {
//                     if ( field.position == p )
//                         return field;
//                 }
//             }
//             // Not Found - send default field
//             CsvField defaultField = new CsvField();
//             defaultField.position = p;
// 
//             // Name can be deduced from the headers if we have read an Headers Line
//             string headerName;
//             if ( _Headers != null && _Headers.Length >= p && (headerName = _Headers[p-1].Trim()).Length > 0 )
//                 defaultField.xmlFieldName = headerName;
//             else
//                 defaultField.xmlFieldName = "field" + p.ToString();
//             defaultField.xmlFieldName = defaultField.xmlFieldName.Replace( ' ', '_' ); // no blank space in field names
// 
//             defaultField.xmlFieldType = CsvFieldTypeEnum.String;
//             return defaultField;
//         }
// 
//         //// Interface
//         #region IETLStep Members
// 
//         public void Init()
//         {
//             // Transmit the relevant settings to the CSV Reader
//             _CSVReader.InputFileName = _Settings.inputFile;
//             if ( _Settings.separator != null && _Settings.separator.Length > 0 )
//                 _CSVReader.FieldSeparator = _Settings.separator[0];
//             if ( _Settings.escape != null && _Settings.escape.Length > 0 )
//                 _CSVReader.FieldEscape = _Settings.escape[0];
// 
//             // We try to open the input file to see if we have any problem doing so
//             try
//             {
//                 FileStream stream = File.OpenRead( _CSVReader.InputFileName );
//                 stream.Close();
//             }
//             catch(Exception ex)
//             {
//                 throw new Exception( "Error: can not open input CSV File for Reading", ex );
//             }
//         }
// 
//         #endregion
//     }

    // CSVExtract is a Pullable Extractor
    // i.e. records are pulled directly by the Queue
    [SettingsType(typeof(Settings), "_Settings")]
    public class CSVLoader : ILoadStep
    {
        // **** Injectable Dependency to modify lah **** 
        public string Name { get; set; }
        public IETLQueue SourceQueue { get; set; }

        // Members
        private Settings _Settings { get; set; }
        private StreamWriter _Writer;
        private string _TimeStampFormat = "[dd/MM/yyyy HH:mm:ss.fff]"; // default format
        private string _Separator = "";
        private int _RecordsProcessedCount = 0;

        public void Init()
        {
            _Separator = _Settings.separator;
            LogManager.Instance.Log("the separator is: " + _Separator);
            
            // get the file
            _Writer = new StreamWriter(_Settings.outputFile);
        }

        public void Start() // start in its own thread
        {
            LogManager.Instance.Log("Load/Debug/" + Name + ": starting step");

            // write the header if needed
            string lineToWrite = "";
            if (_Settings.headerLine)
            {
                bool first = true;
                foreach (CsvField field in _Settings.csvFields)
                {
                    if (first)  {lineToWrite = field.xmlFieldName;          first = false;}
                    else        {lineToWrite = lineToWrite + _Separator + field.xmlFieldName; }
                }
            }
            try                         {_Writer.WriteLine(lineToWrite);}
            catch (System.Exception ex) { LogManager.Instance.Log("Exception while writing the header line" + ex) ;}


            while (SourceQueue.HasMore)
            {
                Record record = SourceQueue.Dequeue();
                if (record != null)
                    Load(record);
            }

            // Dispose the writer when everything is done
            _Writer.Dispose();
            LogManager.Instance.Log("Load/Debug/" + Name + ": step finished - "
                    + _RecordsProcessedCount + " record(s) loaded");
        }

        private void Load(Record record)
        {
            string lineToWrite = "";
            bool first = true;
            // Scan through all the returned fields
            foreach ( CsvField field in _Settings.csvFields )
            {
                string recordToWrite = (record.Fields[field.xmlFieldName]).ToString();

                if (field.xmlFieldType.type == CsvFieldTypeEnum.Date)
                {
                    _TimeStampFormat = field.xmlFieldType.format;
                    DateTime dt = (DateTime)record.Fields[field.xmlFieldName];
                    string theFormat = "{0:" + _TimeStampFormat + "}";
                    recordToWrite = String.Format(theFormat, dt);
                }
                if (first)
                {
                    first = false;
                    lineToWrite = recordToWrite;
                } 
                else
                {
                    lineToWrite = lineToWrite + _Separator + recordToWrite;
                }
            }
   
            try
            {
                _Writer.WriteLine(lineToWrite);
                if (RecordLoaded != null)
                {
                    RecordLoaded(this, record, new DebugLoadSuccessReport());
                }
                
            }
            catch (System.Exception ex)
            {
                if ( RecordLoaded != null )
                    RecordNotLoaded( this, record, new DebugLoadFailureReport(ex) );
                throw ex;
            }
        }

        public event RecordLoadedEventHandler RecordLoaded;
        public event RecordNotLoadedEventHandler RecordNotLoaded;
    }

    class DebugLoadSuccessReport : ILoadSuccessReport
    {
        public string SuccessMessage
        {
            get { return "SUCCESS"; }
        }
    }

    class DebugLoadFailureReport : ILoadFailureReport
    {
        private Exception _Exception;

        public DebugLoadFailureReport(Exception ex)
        {
            _Exception = ex;
        }

        public string FailureMessage { get { return "Unexpected error"; } }
        public Exception FailureException { get { return _Exception; } }
    }
}
