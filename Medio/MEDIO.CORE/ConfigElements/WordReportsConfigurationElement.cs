using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using sophis.configuration;

namespace MEDIO.CORE.ConfigElements
{
    [Description("Configuration parameters for producing word reports")]
    public class WordReportsConfigurationElement : SophisConfigurationSection
    {
        public const string DEFAULT_NAME = "WordReports";

        [Description("Report Template Path")]
        [ConfigurationProperty("WordTemplatePath", IsRequired = true, DefaultValue = "")]
        public String WordTemplatePath
        {
            get { return (String)base["WordTemplatePath"]; }
            set { base["WordTemplatePath"] = value; }
        }

        [Description("Destination path where reports will be stored")]
        [ConfigurationProperty("DestinationPath", IsRequired = true, DefaultValue = "")]
        public String DestinationPath
        {
            get { return (String)base["DestinationPath"]; }
            set { base["DestinationPath"] = value; }
        }

        [Description("Indicate whether to produce word reports or not")]
        [ConfigurationProperty("GenerateReports", IsRequired = true, DefaultValue = true)]
        public bool GenerateReports
        {
            get { return (bool)base["GenerateReports"]; }
            set { base["GenerateReports"] = value; }
        }

    }
}
