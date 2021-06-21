using System;
using System.ComponentModel;
using System.Configuration;
using sophis.configuration;

namespace sophis.quotesource.configuration
{
    public class CFGAdapterConfigurationGroup : AdapterConfigurationGroup
    {
        //DPH
        //public override void RegisterSection()
        public void RegisterSection()
        {
            Sections.Add(CommonAdapterSection.SECTION_NAME, new CFGCommonAdapterSection());
            Sections.Add(CFGAdapterSection.SECTION_NAME, new CFGAdapterSection());
            //Sections.Add(DBFSourceSection.SECTION_NAME, new DBFSourceSection());
            Sections.Add(SQLSourceSection.SECTION_NAME, new SQLSourceSection());
        }

        public CFGAdapterSection CFGAdapterConfiguration
        {
            get { return Sections[CFGAdapterSection.SECTION_NAME] as CFGAdapterSection; }
        }

        //public DBFSourceSection DBFSourceConfiguration
        //{
        //    get { return Sections[DBFSourceSection.SECTION_NAME] as DBFSourceSection; }
        //}

        public SQLSourceSection SQLSourceConfiguration
        {
            get { return Sections[SQLSourceSection.SECTION_NAME] as SQLSourceSection; }
        }

    }

    public enum SourceFormatType
    {
        DATABASE
    };

    public class CFGCommonAdapterSection : CommonAdapterSection
    {
        public CFGCommonAdapterSection()
        {
            //DPH
            //ServiceKind = "CFG";
            SourceKind = "CFG";
            AdapterName = "CFG";
        }

        public override bool UsePermissionService
        {
            get { return false; }
            set
            {
                if (true == value)
                {
                    throw new ArgumentException("CFGAdapter has no external permission service.");
                }
                base.UsePermissionService = false;
            }
        }
    }

    public class CFGAdapterSection : SophisConfigurationSection
    {
        public const string SECTION_NAME = "CFGAdapter";

        [Description(@"Enable verbose output, enable this will generate large amount debug infomation")]
        [ConfigurationProperty("VerboseMode", DefaultValue = "false")]
        public bool VerboseMode
        {
            get { return (bool)this["VerboseMode"]; }
            set { this["VerboseMode"] = value; }
        }

        [Description(@"For Shanghai and Shenzhen Exchange data file, the first record is a special record which contains time 
            and date information. Choose here in which format the time and date are parsed")]
        [ConfigurationProperty("SourceFormat", DefaultValue = SourceFormatType.DATABASE)]
        public SourceFormatType SourceFormat
        {
            get { return (SourceFormatType)this["SourceFormat"]; }
            set { this["SourceFormat"] = value; }
        }

        [Description(@"Frequence of reading data file in second")]
        [ConfigurationProperty("RefreshInterval", DefaultValue = 1L)]
        public long RefreshInterval
        {
            get { return (long)this["RefreshInterval"]; }
            set { this["RefreshInterval"] = value; }
        }
    }

    //public class DBFSourceSection : SophisConfigurationSection
    //{
    //    public const string SECTION_NAME = "DBF";

    //    [DescriptionAttribute(@"Location of CFG data file")]
    //    [ConfigurationProperty("CFGDataFilePath", DefaultValue = "")]
    //    public string CFGDataFilePath
    //    {
    //        get { return (string)this["CFGDataFilePath"]; }
    //        set { this["CFGDataFilePath"] = value; }
    //    }

    //    [DescriptionAttribute(@"Switch if a different user is needed to logon to access data files")]
    //    [ConfigurationProperty("NeedImpersonation", DefaultValue = false)]
    //    public bool NeedImpersonation
    //    {
    //        get { return (bool)this["NeedImpersonation"]; }
    //        set { this["NeedImpersonation"] = value; }
    //    }

    //    [DescriptionAttribute(@"Username to access network location")]
    //    [ConfigurationProperty("ImUsername", DefaultValue = "")]
    //    public string ImUsername
    //    {
    //        get { return (string)this["ImUsername"]; }
    //        set { this["ImUsername"] = value; }
    //    }

    //    [DescriptionAttribute(@"Domain to access network location")]
    //    [ConfigurationProperty("ImDomain", DefaultValue = "")]
    //    public string ImDomain
    //    {
    //        get { return (string)this["ImDomain"]; }
    //        set { this["ImDomain"] = value; }
    //    }

    //    [DescriptionAttribute(@"Password to access network location")]
    //    [ConfigurationProperty("ImPassword", DefaultValue = "")]
    //    public string ImPassword
    //    {
    //        get { return (string)this["ImPassword"]; }
    //        set { this["ImPassword"] = value; }
    //    }

    //    [Description(@"Location to store intermidate files")]
    //    [ConfigurationProperty("TempFileFolder", DefaultValue = "./dataTemp")]
    //    public string TempFileFolder
    //    {
    //        get { return (string)this["TempFileFolder"]; }
    //        set { this["TempFileFolder"] = value; }
    //    }
    //}

    public class SQLSourceSection : SophisConfigurationSection
    {
        public const string SECTION_NAME = "SQL";

        [DescriptionAttribute(@"Database server instance name")]
        [ConfigurationProperty("DatabaseInstance", DefaultValue = "")]
        public string DatabaseInstance
        {
            get { return (string)this["DatabaseInstance"]; }
            set { this["DatabaseInstance"] = value; }
        }

        [DescriptionAttribute(@"Database username")]
        [ConfigurationProperty("Username", DefaultValue = "")]
        public string Username
        {
            get { return (string)this["Username"]; }
            set { this["Username"] = value; }
        }

        [DescriptionAttribute(@"Database password")]
        [ConfigurationProperty("Password", DefaultValue = "")]
        public string Password
        {
            get { return (string)this["Password"]; }
            set { this["Password"] = value; }
        }
    }
}
