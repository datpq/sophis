using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;



namespace SophisETL.Common.GlobalSettings
{
    internal class SettingsFileParser
    {
        private List<GlobalSetting> _Settings = new List<GlobalSetting>();
        public List<GlobalSetting> Settings { get { return _Settings; } }


        public SettingsFileParser()
        {
        }

        public void Parse( string settingsFilePath )
        {
            using ( StreamReader reader = new StreamReader( File.OpenRead( settingsFilePath ) ) )
            {
                Regex settingRegex = new Regex( @"^[ \t]*([a-zA-Z0-9_]+)[ \t]*=[ \t]*(.*)[ \t]*$" );
                Regex includeRegex = new Regex( @"^[ \t]*include\(([\w\\:\.\- ]+)\)[ \t]*$" );
                while ( !reader.EndOfStream )
                {
                    Match aMatch;

                    string line = reader.ReadLine();
                    if ( line.Trim().Length == 0 )
                        continue; // empty line
                    if ( line.Trim().StartsWith( "#" ) )
                        continue; // comment
                    if ( ( aMatch = includeRegex.Match( line ) ).Success )
                    {
                        // recursively include other parameters file
                        Parse( aMatch.Groups[1].Value );
                        continue;
                    }

                    Match settingMatch = settingRegex.Match( line );
                    if ( !settingMatch.Success )
                        throw new Exception( "Settings File Syntax: " + line + " is not recognized as a parameter (name = value) definition" );

                    GlobalSetting newSetting = new GlobalSetting();
                    newSetting.Name  = settingMatch.Groups[1].Value;
                    newSetting.Value = settingMatch.Groups[2].Value.Trim(); //AdB,20110503: in the regex, .* supersedes the last [ \t]* so we still need to trim the end
                    _Settings.Add( newSetting );
                }
            }
        }
    }
}
