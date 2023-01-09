using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SophisETL.Common.Logger;


namespace SophisETL.Common.GlobalSettings
{
    public class GlobalSettings
    {
        // Singleton, explicitely initialized !
        private static GlobalSettings _Instance;
        public  static GlobalSettings Instance { get { return _Instance; } }
        private GlobalSettings() { }
        // Collection of settings
        private Dictionary<string, GlobalSetting> _Settings = new Dictionary<string,GlobalSetting>();
        public Dictionary<string, GlobalSetting> AllSettings { get { return _Settings; } }

        // Public member
        private string _ETLSettingsFile = "config\\sophis_etl.xml";
        public  string  ETLSettingsFile { get { return _ETLSettingsFile; } }



        // This method must be called one first to initialized the Global Settings Instance
        // and never be called again
        public static GlobalSettings Initialize( string[] cmdLineArgs )
        {
             // Check that we are the first ones to initialize ourselves
             //if ( _Instance != null )
             //  throw new Exception( "GlobalSettings can only be initialized once" );

            // Create the instance
            _Instance = new GlobalSettings();

            // Parse the Command Line Arguments
            CommandLineParser parser = new CommandLineParser();
            parser.Parse( cmdLineArgs );

            // Check if a specific name for the ETL Settings file is defined
            if ( parser.Items.ContainsKey( "X" ) )
            {
                CommandLineItem configurationFileItem = parser.Items["X"];
                if ( configurationFileItem.Value == null )
                    throw new Exception( "Command line syntax: -X must be followed by a configuration file (XML) name" );
                _Instance._ETLSettingsFile = configurationFileItem.Value;
            }

            // Check if a configuration file is defined ( -C ) to load the parameters initial values
            if ( parser.Items.ContainsKey( "C" ) )
            {
                CommandLineItem configurationFileItem = parser.Items["C"];
                if ( configurationFileItem.Value == null )
                    throw new Exception( "Command line syntax: -C must be followed by a configuration file (INI) name" );
                SettingsFileParser settingsFileParser = new SettingsFileParser();
                settingsFileParser.Parse( configurationFileItem.Value );

                foreach ( GlobalSetting setting in settingsFileParser.Settings )
                    _Instance.AddSetting( setting );
            }

            // Override the Settings from the file with command line if necessary
            // form is -Pname value
            foreach ( CommandLineItem item in parser.Items.Values )
            {
                if ( item.Option.StartsWith( "P" ) )
                {
                    GlobalSetting newSetting = new GlobalSetting();
                    newSetting.Name = item.Option.Substring( 1 );
                    newSetting.Value = item.Value;
                    _Instance.AddSetting( newSetting );
                }
            }

            // Initialize the Variable String manager
            CSxVariableTokenRepository tokenRepository = CSxVariableStringManager.Instance.GetTokenRepository();
            foreach ( string key in _Instance._Settings.Keys )
            {
                object tokenValue = _Instance._Settings[key].Value;
                tokenRepository.AddToken( new CSxVariableStringTokenImpl( key, Convert.ToString( tokenValue) ) );

                // handle special case of settings with Date
                if ( key.ToLower().Contains("date") && tokenValue != null )
                {
                    try
                    {
                        // try to replace the value at run-time with a datein YYYYMMDD format
                        DateTime tokenValueAsDate = DateTime.ParseExact( tokenValue.ToString(), "yyyyMMdd", new DateTimeFormatInfo() );
                        if ( tokenValueAsDate != null )
                            tokenValue = tokenValueAsDate;
                    }
                    catch ( Exception ex )
                    {
                        ex.GetType(); // to avoid a warning
                    }
                }

                // for dates, we add more formats
                if ( tokenValue is DateTime )
                {
                    DateTime tokenDate = (DateTime) tokenValue;
                    tokenRepository.AddToken( new CSxVariableStringTokenImpl( key + "_XMLDate", tokenDate.ToString( @"yyyy\-MM\-dd"  ) ) );
                    tokenRepository.AddToken( new CSxVariableStringTokenImpl( key + "_SQLDate", "'"+tokenDate.ToString( @"dd\-MMM\-yy" ).ToUpper()+"'" ) );
                }
            }

            return _Instance;
        }



        // Add a new setting to the internal list, previously existing one will be replaced
        private void AddSetting( GlobalSetting setting )
        {
            if ( _Settings.ContainsKey( setting.Name ) )
                _Settings.Remove( setting.Name );
            _Settings.Add( setting.Name, setting );
        }

        public object GetSetting( string settingName )
        {
            return GetSetting( settingName, null );
        }
        public object GetSetting( string settingName, object @default )
        {
            if ( _Settings.ContainsKey( settingName ) )
                return _Settings[settingName].Value;
            else
                return @default;
        }

        // This method will replace the settings embedded into a text string
        // useful to add runtime settings to the XML files
        public string ReplaceSettings( string text )
        {
            return CSxVariableStringManager.Instance.GetStringFiller().FillVariableString( text );
        }
    }

    public class GlobalSetting
    {
        public string Name  { get; set; }
        public string Value { get; set; }
    }
}
