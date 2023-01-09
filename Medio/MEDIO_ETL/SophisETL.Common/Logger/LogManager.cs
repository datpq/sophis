using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SophisETL.Common.Logger
{
    public class LogManager
    {
        // we are a singleton
        private static LogManager _Instance = null;
        public static LogManager Instance { get { return _Instance ?? ( _Instance = new LogManager() ); } }
        private LogManager() { }

        // Log elements
        readonly object _lock = new object(); // lock on the stream
        private StreamWriter _LogStream; // do not access directly! use the property
        private StreamWriter LogStream
        {
            get
            {
                return _LogStream ??
                    ( _LogStream = new StreamWriter( File.Open( LogFileName, ClearFile ? FileMode.Create : FileMode.Append, FileAccess.Write, FileShare.ReadWrite ) ) );
            }
        }

        // Properties
        public string LogFileName         { get { return _LogFileName; } set { _LogFileName = value; } }
        public bool   WriteToDebugConsole { get; set; }
        public string TimeStampFormat     { get { return _TimeStampFormat; } set { _TimeStampFormat = value; } }
        public bool   DebugMode           { get; set; }
        public bool   ClearFile           { get; set; }

        // Members
        private string _LogFileName     = "SophisETL.log";
        private string _TimeStampFormat = "[dd/MM/yyyy HH:mm:ss.fff]"; // default format


        // TODO: handle verbosities later...
        public void Log( string message )
        {
            FormatAndLog( message );
        }
        public void LogDebug( string message )
        {
            if ( DebugMode )
                FormatAndLog( "[DBG] " + message );
        }



        private void FormatAndLog( string message )
        {
            String formattedLog = new StringBuilder( 256 )
                .Append( DateTime.Now.ToString(_TimeStampFormat) )
                .Append( " " )
                .Append( message )
                .ToString();

            lock ( _lock )
            {
                LogStream.WriteLine( formattedLog );
                LogStream.Flush();
            }

            // Display on Console if requested
            if ( WriteToDebugConsole )
                System.Diagnostics.Debug.WriteLine( "[LOG] " + message );
        }

    }
}
