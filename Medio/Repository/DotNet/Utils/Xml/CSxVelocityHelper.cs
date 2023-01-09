// -----------------------------------------------------------------------
//  <copyright file="CSxVelocityHelper.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/07</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Utils.Xml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using NVelocity.App;

    public static class CSxVelocityHelper
    {
        public static Stream Execute(string xmlDataFile, string templateFile, IDictionary<string, object> helpers = null)
        {
            using (var template = new FileStream(templateFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var data = new FileStream(xmlDataFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return Execute(data, template, helpers);
                }
            }
        }

        public static Stream Execute(Stream data, Stream template, IDictionary<string, object> helpers = null)
        {
            var reader = new XmlTextReader(data);
            var doc = XDocument.Load(reader);
            data.Position = 0;
            return Execute(doc.Root, template, helpers);
        }

        public static Stream Execute(XElement element, Stream template, IDictionary<string, object> helpers = null)
        {
            var reader = new StreamReader(template);
            var strBuilder = new StringBuilder();
            var context = new CSxVelocityXmlContext(element) {ReturnXmlValidValues = true};
            
            Stream output;
            var velEngine = new VelocityEngine();

            velEngine.Init();
            context.Put("String", new CSxVelocityStringHelper());
            context.Put("Numeric", new CSxVelocityNumericHelper());
            if (helpers != null)
            {
                foreach (var helper in helpers)
                {
                    context.Put(helper.Key, helper.Value);
                }
            }
            
            using (var writer = new StringWriter(strBuilder))
            {
                var res = velEngine.Evaluate(context, writer, "inline", reader);
                if (!res)
                    throw new Exception("Error evaluating info in Template File!");

                output = new MemoryStream(Encoding.UTF8.GetBytes(strBuilder.ToString()));
            }

            return output;
        }
    }
}