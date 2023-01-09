// -----------------------------------------------------------------------
//  <copyright file="CSxVelocityXmlContext.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/07</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Utils.Xml
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;
    using NVelocity.Context;

    public class CSxVelocityXmlContext : AbstractContext
    {
        #region Fields

        private readonly IEnumerable<XAttribute> _attributes;
        private readonly IDictionary<string, object> _context;
        private readonly XElement _element;
        private readonly IEnumerable<XElement> _elements;

        #endregion

        #region Constructor

        public CSxVelocityXmlContext(XElement element)
        {
            _element = element;
            _elements = element.Elements();
            _attributes = element.Attributes();
            _context = new Dictionary<string, object>();
        }

        #endregion

        #region properties

        public override int Count
        {
            get { return _context.Count + _elements.Count() + _attributes.Count(); }
        }

        public bool ReturnXmlValidValues { get; set; }

        #endregion

        #region Methods

        public override bool InternalContainsKey(object key)
        {
            var index = key as string;
            if (string.IsNullOrEmpty(index)) return false;

            return _element.Attribute(index) != null || _element.Element(index) != null;
        }

        public override object InternalGet(string key)
        {
            string index = key;
            if (string.IsNullOrEmpty(index)) return false;

            object obj;
            XAttribute a = _element.Attribute(index);
            if (a != null)
            {
                return GetValidatedValue(a.Value);
            }

            if (_context.TryGetValue(index, out obj))
            {
                return obj;
            }

            IEnumerable<XElement> es = _element.Elements(index);
            IList<XElement> xElements = es as IList<XElement> ?? es.ToList();
            if (xElements.Count > 1)
            {
                return xElements.Select(x => new CSxVelocityXmlContext(x) {ReturnXmlValidValues = ReturnXmlValidValues});
            }

            XElement e = _element.Element(index);
            if (e != null)
            {
                return new CSxVelocityXmlContext(e) {ReturnXmlValidValues = ReturnXmlValidValues};
            }

            return null;
        }

        public override object[] InternalGetKeys()
        {
            IEnumerable<string> l = _attributes.Select(x => x.Name.ToString());
            l = l.Concat(_elements.Select(x => x.Name.ToString()));
            l = l.Concat(_context.Keys);
            return l.Cast<object>().ToArray();
        }

        public override object InternalPut(string key, object value)
        {
            _context[key] = value;

            return value;
        }

        public override object InternalRemove(object key)
        {
            var index = key as string;
            if (string.IsNullOrEmpty(index)) return null;

            if (_context.ContainsKey(index))
            {
                object o = _context[index];
                _context.Remove(index);
                return o;
            }

            return null;
        }

        public override string ToString()
        {
            return GetValidatedValue(_element.Value);
        }

        private string GetValidatedValue(string value)
        {
            if (ReturnXmlValidValues)
            {
                if (value.IndexOf('&') > 0)
                {
                    value = value.Replace("&", "&amp;");
                }
                if (value.IndexOf('<') > 0)
                {
                    value = value.Replace("&", "&lt;");
                }
                if (value.IndexOf('>') > 0)
                {
                    value = value.Replace("&", "&gt;");
                }
            }
            return value;
        }

        public static implicit operator string(CSxVelocityXmlContext c)
        {
            return c==null ? string.Empty : c.ToString();
        }

        #endregion
    }
}