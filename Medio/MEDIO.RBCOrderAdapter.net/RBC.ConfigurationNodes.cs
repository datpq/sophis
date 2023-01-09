using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.configuration;
using System.Configuration;
using System.ComponentModel;


namespace MEDIO.RBCOrderAdapter
{

    public class RBC_FTPConfigurationNode : sophis.configuration.ConfigurationNode
    {

        public override string Name
        {
            get { return "RBC_FTPConfiguration"; }
        }

        public override void RegisterSectionsAndGroups(System.Configuration.Configuration configuration)
        {
            RBCConfigurationSectionGroup group = RBCConfigurationSectionGroup.Init(configuration);
        }

    }

    public class RBCConfigurationSectionGroup : ConfigurationSectionGroup
    {
        private static RBCConfigurationSectionGroup _current;

        //  Sections Name
        public const string RBC_SECTION_NAME = "RBCConfiguration";
        public const string FTP_SECTION_NAME = "FTPConnection";
        public const string EVENTS_SECTION_NAME = "EventsConfiguration";
        //  Group Name
        public const string GROUP_NAME = "RBC";

        public static RBCConfigurationSectionGroup Current
        {
            get { return _current ?? (_current = Init(ProgramConfiguration.Current.Configuration)); }
            set { _current = value; }
        }


        public static ConfigurationSectionList RBCFileSection
        {
            get
            {
                return Current.Sections[RBC_SECTION_NAME] as ConfigurationSectionList;
            }
        }

        public static FTPConfigurationSection RBCSectionFTP
        {
            get
            {
                return Current.Sections[FTP_SECTION_NAME] as FTPConfigurationSection;
            }
        }

        public static WFEventsConfigurationSection WfEventsSection
        {
            get
            {
                return Current.Sections[EVENTS_SECTION_NAME] as WFEventsConfigurationSection;
            }

        }
        public static RBCConfigurationSectionGroup Init(System.Configuration.Configuration config)
        {
            RBCConfigurationSectionGroup group = config.FindOrCreateGroup<RBCConfigurationSectionGroup>(GROUP_NAME);
            group.FindOrCreateSection<ConfigurationSectionList>(RBC_SECTION_NAME);
            group.FindOrCreateSection<FTPConfigurationSection>(FTP_SECTION_NAME);
            group.FindOrCreateSection<WFEventsConfigurationSection>(EVENTS_SECTION_NAME);
            return group;
        }
    }


    public class FTPConfigurationSection : SophisConfigurationSection
    {

        [Description(@"FTP URL")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("FtpUrl")]
        public string FtpUrl
        {
            get { return (string)this["FtpUrl"]; }
            set { this["FtpUrl"] = value; }
        }

        [Description(@"FTP Port")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("FtpPort")]
        public int FtpPort
        {
            get { return (int)this["FtpPort"]; }
            set { this["FtpPort"] = value; }
        }


        [Description(@"FTP Username")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("FtpUsername")]
        public string FtpUsername
        {
            get { return (string)this["FtpUsername"]; }
            set { this["FtpUsername"] = value; }
        }

        [Description(@"FTP Password")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("FtpPassword")]
        public string FtpPassword
        {
            get { return (string)this["FtpPassword"]; }
            set { this["FtpPassword"] = value; }
        }

    }

    [Description(@"RBC Folders")]
    public class ConfigurationSectionList : SophisConfigurationSection
    {
        [Description(@"Order Adapter instance name")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("OrderAdapterName", DefaultValue = "RBC")]
        public string OrderAdapterName
        {
            get { return (string)this["OrderAdapterName"]; }
            set { this["OrderAdapterName"] = value; }
        }

        [Description(@"Folder containing the files sent to RBC FTP")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("ToRBCFolder")]
        public string ToRBCFolder
        {
            get { return (string)this["ToRBCFolder"]; }
            set { this["ToRBCFolder"] = value; }
        }

        [Description(@"Path of RBC FTP")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("FromRBCFolder")]
        public string FromRBCFolder
        {
            get { return (string)this["FromRBCFolder"]; }
            set { this["FromRBCFolder"] = value; }
        }

        [Description(@"Extension of files sent to RBC")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("OutFileExtension", DefaultValue = "csv")]
        public string OutFileExtension
        {
            get { return (string)this["OutFileExtension"]; }
            set { this["OutFileExtension"] = value; }
        }


        [Description(@"Extension used for NonAcknowledgement files")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("NACKFileExtension", DefaultValue = "nack")]
        public string NACKFileExtension
        {
            get { return (string)this["NACKFileExtension"]; }
            set { this["NACKFileExtension"] = value; }
        }

        [Description(@"Extension used for Acknowledgement files")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("ACKFileExtension", DefaultValue = "ack")]
        public string ACKFileExtension
        {
            get { return (string)this["ACKFileExtension"]; }
            set { this["ACKFileExtension"] = value; }
        }

        [Description(@"Extension used for Execution files")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("EXECFileExtension",DefaultValue = "csv")]
        public string EXECFileExtension
        {
            get { return (string)this["EXECFileExtension"]; }
            set { this["EXECFileExtension"] = value; }
        }


        [Description(@"File separator")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("FileSeparator", DefaultValue = ",")]
        public char FileSeparator
        {
            get { return (char)this["FileSeparator"]; }
            set { this["FileSeparator"] = value; }
        }

        [Description(@"Path to RichMarketAdapter sophis file used for executions")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("RMAdapterFilePath")]
        public string RMAdapterFilePath
        {
            get { return (string)this["RMAdapterFilePath"]; }
            set { this["RMAdapterFilePath"] = value; }
        }

        [Description(@"RichMarketAdapter sophis file name")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("RMAdapterFileName", DefaultValue = "sophisFile_1")]
        public string RMAdapterFileName
        {
            get { return (string)this["RMAdapterFileName"]; }
            set { this["RMAdapterFileName"] = value; }
        }

        [Description(@"RichMarketAdapter sophis file extension")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("RMAdapterFileExt", DefaultValue = "sf")]
        public string RMAdapterFileExt
        {
            get { return (string)this["RMAdapterFileExt"]; }
            set { this["RMAdapterFileExt"] = value; }
        }

        [Description(@"RichMarketAdapter List of order statuses used for rejecting overallocated executions. Values should be separated with ';' .")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("ExecutionRejectOrderStatuses", DefaultValue = "Filled;Completed")]
        public string ExecutionRejectOrderStatuses
        {
            get { return (string)this["ExecutionRejectOrderStatuses"]; }
            set { this["ExecutionRejectOrderStatuses"] = value; }
        }
    }


    public class WFEventsConfigurationSection : SophisConfigurationSection
    {
        [Description(@"Flag to ignore usage of ACK/NACK Files")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("UseACKFiles")]
        public bool UseACKFiles
        {
            get { return (bool)this["UseACKFiles"]; }
            set { this["UseACKFiles"] = value; }
        }

        [Description(@"Execution workflow event")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("ExecutionEvent", DefaultValue = "New Ticket")]
        public string ExecutionEvent
        {
            get { return (string)this["ExecutionEvent"]; }
            set { this["ExecutionEvent"] = value; }
        }

        [Description(@"Aknowledgement workflow event")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("AknowledgementEvent", DefaultValue = "New")]
        public string AknowledgementEvent
        {
            get { return (string)this["AknowledgementEvent"]; }
            set { this["AknowledgementEvent"] = value; }
        }


        [Description(@"Non Aknowledgement workflow event")]
        [Category(PropertyCategory.TECHNICAL), ConfigurationProperty("NonAknowledgementEvent", DefaultValue = "Order Rejected")]
        public string NonAknowledgementEvent
        {
            get { return (string)this["NonAknowledgementEvent"]; }
            set { this["NonAknowledgementEvent"] = value; }
        }
    }

}
