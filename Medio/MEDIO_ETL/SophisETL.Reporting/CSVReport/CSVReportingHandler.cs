using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
using System.IO;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Reporting;
using SophisETL.Common.ErrorMgt;



using SophisETL.Reporting.CSVReport.Xml;

namespace SophisETL.Reporting.CSVReport
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class CSVReportingHandler : IReportingHandler
    {
        private Settings _Settings { get; set; }
        
        // Internal Members
        private string _TimeStampFormat = "[dd/MM/yyyy HH:mm:ss.fff]"; // default format
        private string _Separator = "";
        // File Acess
        readonly object _lock = new object(); // lock on the stream
        private StreamWriter _ReportWriter; // do not access directly! use the property


        ///////////////////////////////////////////////////////////////////////////////////////////
        #region IReportingHandler Members

        public string Name { get; set; }

        public void Init()
        {
            _Separator = _Settings.separator;            
        }

        private void WriteHeader()
        {
            string lineToWrite = "Time" + _Separator +
                                 "Step" + _Separator +
                                 "Record Key" + _Separator +
                                 "Status" + _Separator +
                                 "Message" + _Separator +
                                 "Exception";
            WriteTheLine(lineToWrite);
        }

        //These records are optionals 
        private string AddRecordFields(Record record)
        {
            if (_Settings.fields == null)
                return "";
            StringBuilder additionalFields = new StringBuilder();
            int i = 0;
            int nb = _Settings.fields.Length;
            foreach (string fieldName in _Settings.fields)
            {
                if (record.Fields.ContainsKey(fieldName))
                {
                    additionalFields.Append(fieldName).Append("[").Append(record.Fields[fieldName].ToString()).Append("]");
                }
                if (i < nb -1)
                {
                    additionalFields.Append(",");
                }
                i++;
            }
            additionalFields.Append(_Separator);
            return additionalFields.ToString();
        }

        public void HandleRecordLoadedEvent( ILoadStep loader, Record record, ILoadSuccessReport report )
        {
            if (ReportType.recordNotLoadedOnly.Equals(_Settings.reportType))
                return;
            // Time; Load Step; Record; Status; Message; Exception
            string lineToWrite = DateTime.Now.ToString(_TimeStampFormat) + _Separator +
                                    loader.Name                             + _Separator +
                                    record.Key                              + _Separator +
                                    AddRecordFields(record)                 +
                                    "Success"                               + _Separator +
                                    report.SuccessMessage                   + _Separator;

            WriteTheLine( lineToWrite );
        }

        public void HandleRecordNotLoadedEvent( ILoadStep loader, Record record, ILoadFailureReport report )
        {
            if (ReportType.recordLoadedOnly.Equals(_Settings.reportType))
                return;
            // Time; Load Step; Record; Status; Message; Exception
            string exception = (report.FailureException != null ? report.FailureException.Message : "");
            string lineToWrite = DateTime.Now.ToString(_TimeStampFormat)    + _Separator +
                                    loader.Name                             + _Separator +
                                    record.Key                              + _Separator +
                                    AddRecordFields(record)                 +
                                    "Failure"                               + _Separator +
                                    report.FailureMessage                   + _Separator +
                                    exception                               + _Separator;

            WriteTheLine( lineToWrite );
        }

        public void ReportETLError( ETLError error )
        {
            string exception = ( error.Exception != null ? error.Exception.Message : "" );
            string step      = ( error.Step != null ? error.Step.Name : "unknown" );
            string record    = ( error.Record != null ? error.Record.Key.ToString() : "Generic" );

            string lineToWrite = DateTime.Now.ToString( _TimeStampFormat ) + _Separator +
                                    step + _Separator +
                                    record + _Separator +
                                    "Failure" + _Separator +
                                    "Error in the Chain: " + error.Message + _Separator +
                                    exception + _Separator;

            WriteTheLine( lineToWrite );
        }

        // for the exception management
        private void WriteTheLine(string lineToWrite)
        {
            LogManager log = LogManager.Instance;
            try
            {
                if (_ReportWriter == null)
                {
                    // get the file            
                    _ReportWriter = new StreamWriter(File.Open(_Settings.reportName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                    WriteHeader();
                }
                _ReportWriter.WriteLine( lineToWrite );
            }
            catch (System.Exception e)
            {
                log.LogDebug("An error occured during the writing of the document " + _Settings.reportName + ", exception: " + e.ToString());
                _ReportWriter.Close();
                _ReportWriter.Dispose();
            }
        }

        public void Dispose()
        {
            if (false.Equals(_Settings.skipFooter))
            {
                string lineToWrite = DateTime.Now.ToString(_TimeStampFormat) + _Separator +
                                    "**" + _Separator +
                                    "**" + _Separator +
                                    "**" + _Separator +
                                    "End of report " + _Separator + "**" + "\n\n\n";
                WriteTheLine(lineToWrite);
            }

            if (_ReportWriter != null)
            {
                _ReportWriter.Close();
                _ReportWriter.Dispose();
            }

            //if (true.Equals(_Settings.deleteEmptyFile))
            //{
            //    DeleteEmptyFile();
            //}
        }

        private void DeleteEmptyFile()
        {
            // check there is at least one line else we delete it.
            System.IO.FileInfo fi = new System.IO.FileInfo(_Settings.reportName);
            try
            {
                if (fi.Length <= 1)
                    fi.Delete();
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion
    }
}
