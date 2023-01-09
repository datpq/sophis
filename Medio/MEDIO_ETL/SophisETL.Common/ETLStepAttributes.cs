using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SophisETL.Common
{
    // This attribute is used to specify the Type of Settings to inject in the ETL Steps
    [AttributeUsage( AttributeTargets.Class )]
    public class SettingsTypeAttribute : Attribute
    {
        public SettingsTypeAttribute()
        {
        }

        public SettingsTypeAttribute( Type settingsType )
        {
            SettingsType = settingsType;
            SettingsPropertyName = "Settings";
        }

        public SettingsTypeAttribute( Type settingsType, string settingsPropertyName )
        {
            SettingsType = settingsType;
            SettingsPropertyName = settingsPropertyName;
        }

        public Type SettingsType { get; set; }
        public string SettingsPropertyName { get; set; }
    }
}
