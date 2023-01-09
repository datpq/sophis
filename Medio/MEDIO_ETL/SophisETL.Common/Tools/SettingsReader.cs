using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;



namespace SophisETL.Common.Tools
{
    /// <summary>
    /// Factorizes the reading of Settings:
    /// - Open the file
    /// - substitute the global settings
    /// - deserialize the file to the proper object tree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
    public class SettingsReader<T> where T:class
    {
        public static T ReadSettings(string settingsFileName)
        {
            // Load our settings (we replace the parameters "a la volee"
            string settingsContentWithVariables = File.ReadAllText( settingsFileName );
            string settingsContentWithoutVariables = SophisETL.Common.GlobalSettings.GlobalSettings.Instance.ReplaceSettings( settingsContentWithVariables );
            XmlSerializer serializer = new XmlSerializer( typeof( T ) );
            return serializer.Deserialize( new StringReader( settingsContentWithoutVariables ) ) as T;
        }
    }
}
