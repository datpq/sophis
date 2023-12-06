using System.Configuration;

namespace DataTransformation.Settings
{
    public class TransformationIO : ConfigurationElement
    {
        private const string NameKey = "Name";
        private const string TypeKey = "Type";
        private const string InputKey = "InputDir";
        private const string InputFilterKey = "InputFilter";
        private const string OutputKey = "OutputDir";
        private const string OutputFileKey = "OutputFile";
        private const string BackupKey = "BackupDir";
        private const string FailureKey = "FailureDir";
        private const string SendFailureReportKey = "SendFailureReport";
        private const string EmailSubjectKey = "EmailSubject";
        private const string EmailRecipientToKey = "EmailRecipientTo";
        private const string EmailRecipientCCKey = "EmailRecipientCC";
        private const string EmailBodyKey = "EmailBody";

        [ConfigurationProperty(NameKey, IsRequired = true)]
        public string Name {
            get { return (string)this[NameKey]; }
            set { this[NameKey] = value; }
        }

        [ConfigurationProperty(TypeKey, IsRequired = true)]
        public string Type {
            get { return (string)this[TypeKey]; }
            set { this[TypeKey] = value; }
        }

        [ConfigurationProperty(InputKey, DefaultValue = "Input", IsRequired = true)]
        public string InputDir {
            get { return (string)this[InputKey]; }
            set { this[InputKey] = value; }
        }

        [ConfigurationProperty(InputFilterKey, DefaultValue = "*.csv", IsRequired = true)]
        public string InputFilter {
            get { return (string)this[InputFilterKey]; }
            set { this[InputFilterKey] = value; }
        }

        [ConfigurationProperty(OutputKey, DefaultValue ="Output", IsRequired = true)]
        public string OutputDir {
            get { return (string)this[OutputKey]; }
            set { this[OutputKey] = value; }
        }

        [ConfigurationProperty(OutputFileKey, IsRequired = true)]
        public string OutputFile {
            get { return (string)this[OutputFileKey]; }
            set { this[OutputFileKey] = value; }
        }

        [ConfigurationProperty(BackupKey, DefaultValue = "Backup", IsRequired = true)]
        public string BackupDir {
            get { return (string)this[BackupKey]; }
            set { this[BackupKey] = value; }
        }

        [ConfigurationProperty(FailureKey, DefaultValue = "Failure", IsRequired = false)]
        public string FailureDir
        {
            get { return (string)this[FailureKey]; }
            set { this[FailureKey] = value; }
        }

        [ConfigurationProperty(SendFailureReportKey, DefaultValue = false, IsRequired = false)]
        public bool SendFailureReport
        {
            get { return (bool)this[SendFailureReportKey]; }
            set { this[FailureKey] = value; }
        }

        [ConfigurationProperty(EmailSubjectKey, IsRequired = false)]
        public string EmailSubject
        {
            get { return (string)this[EmailSubjectKey]; }
            set { this[EmailSubjectKey] = value; }
        }

        [ConfigurationProperty(EmailRecipientToKey, IsRequired = false)]
        public string EmailRecipientTo
        {
            get { return (string)this[EmailRecipientToKey]; }
            set { this[EmailRecipientToKey] = value; }
        }

        [ConfigurationProperty(EmailRecipientCCKey, IsRequired = false)]
        public string EmailRecipientCC
        {
            get { return (string)this[EmailRecipientCCKey]; }
            set { this[EmailRecipientCCKey] = value; }
        }

        [ConfigurationProperty(EmailBodyKey, IsRequired = false)]
        public string EmailBody
        {
            get { return (string)this[EmailBodyKey]; }
            set { this[EmailBodyKey] = value; }
        }
    }
}
