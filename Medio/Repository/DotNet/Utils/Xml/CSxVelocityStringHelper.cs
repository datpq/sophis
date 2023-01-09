namespace Sophis.Toolkit.Utils.Xml
{
    using System;

    public class CSxVelocityStringHelper
    {
        public string Truncate(string str, int lenght)
        {
            return str.Substring(0, Math.Min(lenght, str.Length));
        }
    }
}