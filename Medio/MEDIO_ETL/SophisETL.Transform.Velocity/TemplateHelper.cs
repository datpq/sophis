using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SophisETL.Transform.Velocity
{
    /// <summary>
    /// This class provides useful methods that can be used in templates
    /// </summary>
    public class TemplateHelper
    {
        // Returns the current date / time
        public DateTime TimeStamp { get { return DateTime.Now; } }

        // Escape a given string so that it will be XML compliant (& -> &amp; etc...)
        public string XmlEscape(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }
    }
}
