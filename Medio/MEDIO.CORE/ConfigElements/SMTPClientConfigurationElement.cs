using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using sophis.configuration;

namespace MEDIO.CORE.ConfigElements
{
    public enum ERecipientListType
    {
        To,
        CC,
        BCC
    }

    [Description("Configuration parameters for mail")]
    public class SmtpClientConfigurationElement : SophisConfigurationSection
    {
        public const string DEFAULT_NAME = "SMTPClient";

        [Description("STMP Host")]
        [ConfigurationProperty("SMTPHost", IsRequired = true, DefaultValue = "")]
        public String SMTPHost
        {
            get { return (String) base["SMTPHost"]; }
            set { base["SMTPHost"] = value; }
        }

        [Description("STMP Port")]
        [ConfigurationProperty("SMTPPort", IsRequired = true, DefaultValue = 25)]
        public int SMTPPort
        {
            get { return (int) base["SMTPPort"]; }
            set { base["SMTPPort"] = value; }
        }

        [Description("Address of the sender of mails")]
        [ConfigurationProperty("From", IsRequired = true, DefaultValue = "")]
        public string From
        {
            get { return (string) base["From"]; }
            set { base["From"] = value; }
        }

        [Description("Username")]
        [ConfigurationProperty("Username", DefaultValue = "")]
        public string Username
        {
            get { return (string) base["Username"]; }
            set { base["Username"] = value; }
        }

        [Description("Password")]
        [ConfigurationProperty("Password", DefaultValue = ""), PasswordPropertyText(true)]
        public string Password
        {
            get { return (string) base["Password"]; }
            set { base["Password"] = value; }
        }

        [Description("Domain")]
        [ConfigurationProperty("Domain", DefaultValue = "")]
        public string Domain
        {
            get { return (string) base["Domain"]; }
            set { base["Domain"] = value; }
        }

        [Description("Enable Secure Sockets Layer")]
        [ConfigurationProperty("EnableSSL", DefaultValue = true)]
        public bool EnableSSL
        {
            get { return (bool) base["EnableSSL"]; }
            set { base["EnableSSL"] = value; }
        }

        [Description("Type of the recipients. Either To or Cc or Bcc")]
        [ConfigurationProperty("RecipientListType", DefaultValue = ERecipientListType.To, IsRequired = false),
         Browsable(true)]
        public ERecipientListType RecipientListType
        {
            get { return (ERecipientListType) base["RecipientListType"]; }
            set { base["RecipientListType"] = value; }
        }


        [Category("MAML"), Description("MAML email address used in the email signature")]
        [ConfigurationProperty("MAMLEmailAddress", DefaultValue = "trading@mediolanum.ie")]
        public string MAMLEmailAddress
        {
            get { return (string)base["MAMLEmailAddress"]; }
            set { base["MAMLEmailAddress"] = value; }
        }

        [Category("MAML"), Description("MAML web address used in the email signature")]
        [ConfigurationProperty("MAMLWebAddress", DefaultValue = "www.mifl.ie")]
        public string MAMLWebAddress
        {
            get { return (string)base["MAMLWebAddress"]; }
            set { base["MAMLWebAddress"] = value; }
        }
    }
}
