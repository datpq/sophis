using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Tools;
using SophisETL.Common.ErrorMgt;
using SophisETL.Extract.SQLServerDBExtract.Xml;



namespace SophisETL.Extract.SQLServerDBExtract
{
    /// <summary>
    /// Data Base record extractor
    /// 
    /// This Extract step is based on a DataBase connection, the Query loading the record can be set by the user.
    /// 
    /// Dynamic analysis of the returned Record Set is performed to create on the fly the record names from the column titles
    /// retured by Oracle.
    /// 
    /// </summary>
    [SettingsType(typeof(Settings), "_Settings")]
    public class SQLServerDBExtract : IExtractStep
    {
        private Settings      _Settings { get; set; }
        private SqlConnection _DBCon;    // opened in Init()
        private SqlCommand    _Command;  // prepared in Init()
        private int           _RecordsExtractedCount = 0;
        private List<DBField> _DBFields;

        #region SQLServerDBExtract Chain Parameters
        //// Setup of the Chain including this extract
        public string    Name         { get; set; }
        // Only an Output Queue exists
        public IETLQueue TargetQueue  { get; set; }
        #endregion




        public void Init()
        {
            // We try to open the DB Connection and fetch the 1st row to see if DB Connection
            // parameters and SQL Syntax are correct
            try
            {
                string connectionString =
                        "Data Source=" + _Settings.dbConnection.instance + ";" +
                        "User Id=" + _Settings.dbConnection.login + ";" +
                        "Password=" + _Settings.dbConnection.password + ";" +
                        "Connection Timeout=" + _Settings.connectionTimeout;
                _DBCon = new SqlConnection( connectionString );
                _DBCon.Open();
            }
            catch ( Exception ex )
            {
                // TODO: replace by proper error reporting framework
                throw new Exception( "Error: can not open connection to specified database", ex );
            }

            // Now let's check the syntax consistency
            try
            {
                _Command = _DBCon.CreateCommand();
                _Command.CommandTimeout = _Settings.queryTimeout;
                _Command.CommandText = _Settings.query;
                //_Command.Prepare(); // this will check the command syntaxt
                // 20111003,AdB: after migration to ODP.NET Prepare() does nothing
                // trying to fetch CommandType will work however (suggested by Johan Baltie)
                // AdB: check if same problem with SQL Server and uncomment next line if this is the case
                // AdB: same problem with SQL Server, let's try commandType
                System.Data.CommandType commandType = _Command.CommandType;
            }
            catch ( Exception ex )
            {
                // TODO: replace by proper error reporting framework
                throw new Exception( "Error: error with the SQL query (" + _Command.CommandText + ")", ex );
            }
        }
        
        public void Start()
        {
            LogManager.Instance.Log( "Extract/DB/" + Name + ": starting step" );

            // Execute the Query
            SqlDataReader reader = _Command.ExecuteReader();
            // Prepare the Record dynamic informations
            StoreRecordInformations( reader );

            while ( reader.Read() )
            {
                Record record = Record.NewRecord( ++ _RecordsExtractedCount );

                // Scan through all the returned fields
                foreach ( DBField field in _DBFields )
                {
                    object value = reader[field.FieldIndex];
                    try
                    {
                        if ( value is DBNull )
                        {
                            record.Fields.Add( field.FieldName, "" );
                            record.Fields.Add( field.FieldName + "_IsNull", true );
                        }
                        else
                        {
                            record.Fields.Add( field.FieldName, Convert.ChangeType( value, field.FieldTargetType ) );
                            record.Fields.Add( field.FieldName + "_IsNull", false );
                        }
                    }
                    catch ( Exception ex )
                    {
                        // report the error
                        ErrorHandler.Instance.HandleError( new ETLError
                        {
                            Exception = ex,
                            Step = this,
                            Message = "On record " + _RecordsExtractedCount + " failed to read properly field " + field.FieldName + " with value " + value
                        } );
                        // and skip it
                        record = null;
                        break;
                    }
                }

                TargetQueue.Enqueue( record );
            }

            LogManager.Instance.Log( "Extract/DB/" + Name + ": step finished - "
                + _RecordsExtractedCount + " record(s) extracted" );
        }


        private void StoreRecordInformations( SqlDataReader reader )
        {
            _DBFields = new List<DBField>();

            for ( int fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++ )
            {
                DBField field    = new DBField();
                field.FieldIndex = fieldIndex;
                field.FieldName  = reader.GetName( fieldIndex );
                field.FieldTargetType = reader.GetFieldType( fieldIndex ); // TODO: convert to .Net native type instead of Oracle

                LogManager.Instance.LogDebug( field.ToString() );

                _DBFields.Add( field );
            }
        }

        private struct DBField
        {
            public int FieldIndex { get; set; }
            public string FieldName { get; set; }
            public Type FieldTargetType { get; set; }

            public override string ToString()
            {
                return "[DBField; Index=" + FieldIndex + "; Name=" + FieldName + "; Type=" + FieldTargetType.Name + "]";
            }
        }
    }
}
