using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using SophisETL.Common.GlobalSettings;
using SophisETL.Common.Logger;


namespace SophisETL.Common
{
    public enum eExistsAction { doNotAdd, replace, suffixNew };

    /// <summary>
    /// A simple Record, must be instanciated through the NewRecord() factory method since there is some initialization
    /// done at creation time (parameters addition for example)
    /// </summary>
    public class Record
    {
        // Data Part
        private object _Key;
        public object Key { get { return _Key; } } // read-only, assigned at creation time

        // Constants
        public static readonly string ETL_PARAMS_FIELDNAME = "ETLParameters";

        // Velocity and other reflexion-friendly framework needs a property here
        private Dictionary<string, object> _Fields = new Dictionary<string, object>();
        public Dictionary<string, object> Fields { get { return _Fields; } }

        public void SafeAdd( string key, object value, eExistsAction actionIfExists )
        {
            if ( !Fields.ContainsKey( key ) )
                Fields.Add( key, value );
            else
                switch ( actionIfExists )
                {
                    default:
                    case eExistsAction.doNotAdd:
                        break;

                    case eExistsAction.replace:
                        Fields.Remove( key );
                        Fields.Add( key, value );
                        break;

                    case eExistsAction.suffixNew:
                        string newKey = key;
                        for ( int i = 2; Fields.ContainsKey( newKey = key + "_" + i ); i++ ) ;
                        Fields.Add( newKey, value );
                        break;
                }
        }

        /// <summary>
        /// Overloaded SafeAdd that will add a new field by prefixing it as many times as necessary
        /// if it already exists!
        /// </summary>
        /// <param name="key">Field Key</param>
        /// <param name="value">Field Value</param>
        /// <param name="prefixIfExists">Prefix added to the key as many times as necessary</param>
        public void SafeAdd( string key, object value, string prefixIfExists )
        {
            string addKey = key;
            while ( Fields.ContainsKey( addKey ) )
                addKey = prefixIfExists + addKey;
            Fields.Add( addKey, value );
        }


        // Factory Part
        private Record( object key )
        {
            _Key = key;
            Fields.Add( ETL_PARAMS_FIELDNAME, ChainParameters );
        }

        public static Record NewRecord( object key )
        {
            // Add all the current parameters in it
            return new Record( key );
        }

        private static Dictionary<string, object> _ChainParameters;
        private static Dictionary<string, object> ChainParameters
        {
            get
            {
                if ( _ChainParameters == null )
                {
                    // Create the Initial Content of the Record
                    _ChainParameters = new Dictionary<string, object>();
                    foreach ( GlobalSetting s in GlobalSettings.GlobalSettings.Instance.AllSettings.Values )
                    {
                        object tokenValue = s.Value;

                        // handle special case of password settings which should not be available to see in the records
                        if ( s.Name.ToLower().Contains( "password" ) && tokenValue != null )
                        {
                            tokenValue = "*****";
                        }
                        // handle special case of settings with Date
                        if ( s.Name.ToLower().Contains( "date" ) && tokenValue != null )
                        {
                            try
                            {
                                // try to replace the value at run-time with a date in YYYYMMDD format
                                DateTime tokenValueAsDate = DateTime.ParseExact( tokenValue.ToString(), "yyyyMMdd", new DateTimeFormatInfo() );
                                if ( tokenValueAsDate != null )
                                    tokenValue = tokenValueAsDate;
                            }
                            catch ( Exception ) { }
                        }

                        _ChainParameters.Add( s.Name, tokenValue );
                    }
                }
                return _ChainParameters;
            }
        }

        // Standard Debug Print of a Record
        public override string ToString()
        {
            StringBuilder recordString = new StringBuilder();

            // basic print of the record
            try
            {
                recordString.Append( "Record[" ).Append( _Key ).AppendLine( "]:" );
                // Build Record Data
                foreach (string fieldKey in Fields.Keys)
                {
                    recordString.Append( "  " ).Append( fieldKey ).Append( " = " );
                    object fieldValue = Fields[fieldKey];

                    // different display depending on the type
                    if ( fieldValue is List<Record> )
                    {
                        recordString.AppendLine( "[ (list of records)" );
                        foreach ( Record rec in (List<Record>) fieldValue )
                            recordString.Append( rec );
                        recordString.AppendLine( "(end of list) ]" );
                    }
                    else if ( fieldValue is List<string> )
                    {
                        recordString.AppendLine( "[ (list of strings)" );
                        foreach ( string str in (List<string>) fieldValue )
                            recordString.AppendLine( str );
                        recordString.AppendLine( "(end of list) ]" );
                    }
                    else if ( fieldValue is System.Collections.ICollection )
                    {
                        recordString.AppendLine( "[ (collection)" );
                        foreach ( object obj in (System.Collections.ICollection) fieldValue )
                            recordString.Append( obj ).AppendLine();
                        recordString.AppendLine( "(end of collection) ]" );
                    }
                    else
                    {
                        recordString.Append( fieldValue ).AppendLine();
                    }
                }
                recordString.AppendLine();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Log("Exception while printing the records: " + ex);
                return "Record[" + _Key + "] - failed to print content";
                //throw; // do not rethrow, we do not want to interrupt the chain for a debug print problem
            }

            return recordString.ToString();
        }
    }
}
