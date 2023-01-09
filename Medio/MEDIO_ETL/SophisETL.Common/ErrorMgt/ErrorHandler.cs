using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SophisETL.Common.Logger;
using SophisETL.Common.Reporting;


namespace SophisETL.Common.ErrorMgt
{
    public class ErrorHandler
    {
        // Singleton
        private static ErrorHandler _Instance;
        public static ErrorHandler Instance { get { return _Instance ?? ( _Instance = new ErrorHandler() ); } }
        private ErrorHandler() { }

        // Members
        private List<IReportingHandler> _ReportingHandlers = new List<IReportingHandler>();



        public void HandleError( ETLError error )
        {
            // Log the error
            string errorMessage = String.Format(
                "{0} ERROR on Step {1}: {2}"
                +"{3}"
                +"{4}",
                error.Record == null ? "GENERAL" : "RECORD",
                error.Step.Name,
                error.Message,
                error.Exception == null ? "" : GetExceptionErrorString( error.Exception ),
                error.Record == null ? "" : GetRecordDescription( error.Record )
            );

            LogManager.Instance.Log( errorMessage );

            // Log on Console
            Console.Out.WriteLine( errorMessage );

            // Dispatch to reporting Handlers
            _ReportingHandlers.ForEach( r => r.ReportETLError( error ) );
        }

        private string GetRecordDescription( Record record )
        {
            return "\n" + record.ToString();
        }

        private string GetExceptionErrorString( Exception exception )
        {
            return String.Format( "\nException Details: {0}\n{1}", exception.Message, exception.ToString() );
        }

        public void AttachReportingHandler( IReportingHandler reportingHandler )
        {
            _ReportingHandlers.Add( reportingHandler );
        }
    }
}
