using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.configuration;
using System.Configuration;
using System.ComponentModel;

namespace RBCInstrumentsPrice
{
    public class RBCConfigurationSectionGroup : ConfigurationSectionGroup
    {
        private static RBCConfigurationSectionGroup _current;

        //  Sections Name
        public const string RBC_SECTION_NAME = "RBCSetup";
        public const string ALLOTMENT_SECTION_NAME = "AllotmentsSetup";
        //  Group Name
        public const string GROUP_NAME = "RBC";

        public static RBCConfigurationSectionGroup Current
        {
            get { return _current ?? (_current = Init(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None))); }
            set { _current = value; }
        }


        public static RBCConfigurationSection RBCSectionConfig
        {
            get
            {
                return Current.Sections[RBC_SECTION_NAME] as RBCConfigurationSection;
            }
        }

        public static AllotmentsSection AllotSectionConfig
        {
            get
            {
                return Current.Sections[ALLOTMENT_SECTION_NAME] as AllotmentsSection;
            }
        }

        public static RBCConfigurationSectionGroup Init(System.Configuration.Configuration config)
        {
            
            RBCConfigurationSectionGroup group = config.FindOrCreateGroup<RBCConfigurationSectionGroup>(GROUP_NAME);
            group.FindOrCreateSection<RBCConfigurationSection>(RBC_SECTION_NAME);
            return group;


        }
    }

    public class AllotmentsSection : ConfigurationSection
    {
        [Description(@"Allotments in scope for CUVAL")]
        [Category("TECHNICAL"), ConfigurationProperty("AllotmentsCUVAL")]
        public string AllotmentsCUVAL
        {
            get { return (string)this["AllotmentsCUVAL"]; }
            set { this["AllotmentsCUVAL"] = value; }
        }

        [Description(@"Allotments in scope for SWAPS")]
        [Category("TECHNICAL"), ConfigurationProperty("AllotmentsSWAPS")]
        public string AllotmentsSWAPS
        {
            get { return (string)this["AllotmentsSWAPS"]; }
            set { this["AllotmentsSWAPS"] = value; }
        }

        [Description(@"Allotments in scope for OPTIONS")]
        [Category("TECHNICAL"), ConfigurationProperty("AllotmentsOPTIONS")]
        public string AllotmentsOPTIONS
        {
            get { return (string)this["AllotmentsOPTIONS"]; }
            set { this["AllotmentsOPTIONS"] = value; }
        }

        [Description(@"Allotments in scope for FUTURES")]
        [Category("TECHNICAL"), ConfigurationProperty("AllotmentsFUTURES")]
        public string AllotmentsFUTURES
        {
            get { return (string)this["AllotmentsFUTURES"]; }
            set { this["AllotmentsFUTURES"] = value; }
        }

        [Description(@"Allotments in scope for price multiply with 100")]
        [Category("TECHNICAL"), ConfigurationProperty("AllotmentsPriceMultiply")]
        public string AllotmentsPriceMultiply
        {
            get { return (string)this["AllotmentsPriceMultiply"]; }
            set { this["AllotmentsPriceMultiply"] = value; }
        }

        [Description(@"Allotments in scope for price divide with 100")]
        [Category("TECHNICAL"), ConfigurationProperty("AllotmentsPriceDivide")]
        public string AllotmentsPriceDivide
        {
            get { return (string)this["AllotmentsPriceDivide"]; }
            set { this["AllotmentsPriceDivide"] = value; }
        }
    }




    public class RBCConfigurationSection : ConfigurationSection
    {
        [Description(@"From RBC path")]
        [Category("TECHNICAL"), ConfigurationProperty("InputFolder")]
        public string InputFolder
        {
            get { return (string)this["InputFolder"]; }
            set { this["InputFolder"] = value; }
        }


        [Description(@"NotFoundInstruments file path")]
        [Category("TECHNICAL"), ConfigurationProperty("FileForNotFoundInstruments")]
        public string FileForNotFoundInstruments
        {
            get { return (string)this["FileForNotFoundInstruments"]; }
            set { this["FileForNotFoundInstruments"] = value; }
        }


        [Description(@"Loading query for instruments in scope")]
        [Category("TECHNICAL"), ConfigurationProperty("InScopeQuery")]
        public string InScopeQuery
        {
            get { return (string)this["InScopeQuery"]; }
            set { this["InScopeQuery"] = value; }
        }

        [Description(@"Path for processed files")]
        [Category("TECHNICAL"), ConfigurationProperty("ProcessedFolder")]
        public string ProcessedFolder
        {
            get { return (string)this["ProcessedFolder"]; }
            set { this["ProcessedFolder"] = value; }
        }

        [Description(@"")]
        [Category("TECHNICAL"), ConfigurationProperty("User")]
        public string User
        {
            get { return (string)this["User"]; }
            set { this["User"] = value; }
        }
        [Description(@"")]
        [Category("TECHNICAL"), ConfigurationProperty("Password")]
        public string Password
        {
            get { return (string)this["Password"]; }
            set { this["Password"] = value; }
        }
        [Description(@"")]
        [Category("TECHNICAL"), ConfigurationProperty("Server")]
        public string Server
        {
            get { return (string)this["Server"]; }
            set { this["Server"] = value; }
        }
    }


}
