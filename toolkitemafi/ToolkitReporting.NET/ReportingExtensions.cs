using System;
using System.Linq;
using System.Xml.Linq;

// ReSharper disable once CheckNamespace
namespace Eff.ToolkitReporting.NET
{
    public static class ReportingExtensions
    {
        public static readonly XNamespace XslNs = "http://www.w3.org/1999/XSL/Transform";
        public static XNamespace SsNs;
        public static void ReadNamespaces(this XDocument docExcelTemplate)
        {
            if (SsNs == null)
            {
                var ssNsAttr = docExcelTemplate.Root.Attributes()
                    .Single(x => x.IsNamespaceDeclaration && x.Name.LocalName == "ss");
                SsNs = ssNsAttr.Value;
            }
        }

        public static void IncrementAttribute(this XElement elem, string attribute, string formula)
        {
            var attrElem = elem.Attribute(SsNs + attribute);
            if (attrElem == null)
            {
                var childAttrElem = elem.Elements(XslNs + "attribute").SingleOrDefault(x => x.Attribute("name").Value == string.Format("ss:{0}", attribute));
                if (childAttrElem == null) return;
                var attr = childAttrElem.Elements(XslNs + "value-of").Single().Attribute("select");
                if (attr != null)
                {
                    attr.Value = string.Format("{0}+{1}", attr.Value, formula);
                }
                else
                {
                    throw new Exception(string.Format("Attribute {0} not found", attribute));
                }
                //}
                //else
                //{
                //    elem.AddFirst(new XElement(XslNs + "attribute",
                //        new XAttribute("name", string.Format("ss:{0}", attribute)),
                //        new XElement(XslNs + "value-of",
                //            new XAttribute("select", formula))));
            }
            else
            {
                var attrVal = attrElem.Value;
                attrElem.Remove();
                elem.AddFirst(new XElement(XslNs + "attribute",
                    new XAttribute("name", string.Format("ss:{0}", attribute)),
                    //new XAttribute("name", SsNs + attribute),
                    new XElement(XslNs + "value-of",
                        new XAttribute("select", string.Format("{0}+{1}", attrVal, formula)))));
            }
        }
    }
}
