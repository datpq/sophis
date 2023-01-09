using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using sophis.configuration;

namespace MEDIO.CORE.ConfigElements
{
    public class ToolkitConfigurationNode : sophis.configuration.ConfigurationNode
    {
        public override string Name
        {
            get { return "BOEmailNotification"; }
        }

        public override void RegisterSectionsAndGroups(System.Configuration.Configuration configuration)
        {
            ConfigurationGroup group = ConfigurationGroup.Init(configuration);
        }
    }

    public class ConfigurationGroup : ConfigurationSectionGroup
    {
        //  Group Name
        public const string GROUP_NAME = "BOEmailNotification";
        private static ConfigurationGroup _current = null;
        public static ConfigurationGroup Current
        {
            get
            {
                if (_current == null &&
                    ProgramConfiguration.Current != null && ProgramConfiguration.Current.Configuration != null)
                {
                    _current = ProgramConfiguration.Current.Configuration.SectionGroups[GROUP_NAME] as ConfigurationGroup;
                }
                return _current;
            }
            set
            {
                _current = value;
            }
        }

        /// <summary>
        /// Register new entries here
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static ConfigurationGroup Init(System.Configuration.Configuration config)
        {
            ConfigurationGroup group = config.FindOrCreateGroup<ConfigurationGroup>(GROUP_NAME);
            group.FindOrCreateSection<SmtpClientConfigurationElement>(SmtpClientConfigurationElement.DEFAULT_NAME);
            return group;
        }


        [Category(PropertyCategory.TECHNICAL)]
        public SmtpClientConfigurationElement SMTPClient
        {
            get { return this.Sections[SmtpClientConfigurationElement.DEFAULT_NAME] as SmtpClientConfigurationElement; }
        }

    }
}
