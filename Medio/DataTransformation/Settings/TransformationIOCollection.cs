using System.Configuration;

namespace DataTransformation.Settings
{
    [ConfigurationCollection(typeof(TransformationIO), AddItemName = TransformationIOKey)]
    public class TransformationIOCollection : ConfigurationElementCollection
    {
        private const string TransformationIOKey = "Trans";
        protected override ConfigurationElement CreateNewElement()
        {
            return new TransformationIO();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TransformationIO)element).Name;
        }
    }
}
