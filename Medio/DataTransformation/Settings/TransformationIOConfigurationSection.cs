using System.Configuration;

namespace DataTransformation.Settings
{
    public class TransformationIOConfigurationSection : ConfigurationSection
    {
        private const string TranformationIOsKey = "TransIOs";

        [ConfigurationProperty(TranformationIOsKey, IsRequired = true)]
        public TransformationIOCollection TransIOs {
            get { return (TransformationIOCollection)this[TranformationIOsKey]; }
            set { this[TranformationIOsKey] = value; }
        }
    }
}

