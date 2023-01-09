using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
//using Sophis.Web.Api;
using SophisETL.Common;
using SophisETL.Common.Logger;
//using SophisETL.Engine.IntegrationService.WebApi;
using SophisETL.Transform.MedioSOAFilter.Xml;
using SophisETL.ISEngine;
using sophis.services;
using sophis.services.xmlns_reporting;

namespace SophisETL.Transform.MedioSOAFilter
{
    [SettingsType(typeof(Settings), "_Settings")]
    class MedioSOAFilter : ITransformStep
    {
        #region private members

        private Settings _Settings { get; set; }
        private List<IETLQueue> m_sourceQueues = new List<IETLQueue>();
        private List<IETLQueue> m_targetQueues = new List<IETLQueue>();
        #endregion

        #region properties
        public List<IETLQueue> SourceQueues
        {
            get { return m_sourceQueues; }
        }

        public List<IETLQueue> TargetQueues
        {
            get { return m_targetQueues; }
        }
        public string Name { get; set; }
        #endregion

        // Start of our thread, we load and push
        public event EventHandler NoMoreRecords;
        private string logClassName = null;
        private IntegrationServiceEngine _ISEngine;

        private bool IsRejected(string xmlResponse)
        {
            return xmlResponse.Contains("elementRejected");
        }

        public void Init()
        {
            logClassName = "[Transform/MedioSOAFilter/" + this.Name + "]";
            LogManager.Instance.Log(logClassName + " Starting Step...");

            // Make sure we have only 1 Source / Target Queue
            if (m_sourceQueues.Count != 1)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have one source queue", Name,
                    GetType().Name));
            }
            if (m_targetQueues.Count != 1)
            {
                throw new Exception(String.Format("Transform step [{1}/{0}] must have only one target Queue", Name,
                    GetType().Name));
            }
            _ISEngine = IntegrationServiceEngine.Instance;
        }

        // check if an object is a numeric type and store the value in dValue
        private bool IsNumeric(object Expression, ref double dValue)
        {
            bool bIsNum = false;
            double dRetNum = .0;
            bIsNum = Double.TryParse(Convert.ToString(Expression),
                System.Globalization.NumberStyles.Any,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out dRetNum);

            dValue = dRetNum;
            return bIsNum;
        }

        public void Start()
        {
            int count = _Settings.soaParameters.Length;
            
            var records = GetRecords();

            foreach (var record in records)
            {
                m_targetQueues[0].Enqueue(record);
                List<SoaMethodParameter> paramList = new List<SoaMethodParameter>(count);
                for (int i = 0; i < count; i++)
                {
                    SoaMethodParameter param = new SoaMethodParameter();
                    var field = _Settings.soaParameters[i].soaParamValue;
                    param.name = _Settings.soaParameters[i].soaParamName;
                    if (param.name.ToUpper().Contains("DATE"))
                    {
                        DateTime fieldValue = (DateTime)record.Fields[field];
                        param.Value = fieldValue.ToString("dd-MM-yy");
                    }
                    else
                    {
                        param.Value = (string)record.Fields[field].ToString();
                    }
                    paramList.Add(param);
                }

                XDocument resultXdoc = null; //To implement _ISEngine.ExecuteMethod(_Settings.soaMethod, paramList);
                if (resultXdoc != null)
                {
                    string xmlMessage = resultXdoc.ToString();
                    //We had to move from WebAPI (not supported).
                    //Some methods have to be re-implemented.

                    //try
                    //{
                    //    if (IsRejected(xmlMessage))
                    //    {
                    //        LogManager.Instance.Log((new XmlISExtractFailureReport(xmlMessage)).FailureMessage);
                    //        NoMoreRecords(this, null);
                    //        return;
                    //    }
                    //    foreach (XElement node in ((resultXdoc.Root.Element(NS.nsR + "default0") as XElement)
                    //        .Elements(NS.nsR + "default0Result")))
                    //    {
                    //        foreach (XNode e in node.Nodes())
                    //        {
                    //            XElement elt = e as XElement;
                    //            double price = 0;
                    //            if (IsNumeric(elt.Value.ToString(), ref price))
                    //            {
                    //                if (price != 0)
                    //                {
                    //                    LogManager.Instance.Log("This record is going to be filterd out, as a price has been found on this day!");
                    //                    m_targetQueues[0].Dequeue();
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    LogManager.Instance.Log(ex.Message + " IsMessage:" + (new XmlISExtractFailureReport(xmlMessage)).FailureMessage);
                    //}
                }
            }
        }

        private List<Record> GetRecords()
        {
            List<Record> theRecordSet = new List<Record>();

            // get all the records and add them to the list (only use the first Queue as there is only one)
            while (m_sourceQueues[0].HasMore)
            {
                Record record = m_sourceQueues[0].Dequeue();
                if (record != null)
                    theRecordSet.Add(record);
            }
            return theRecordSet;
        }

    }

    class XmlISExtractFailureReport
    {
        public XmlISExtractFailureReport(string xmlMessage)
        {
            // parse the XML to get the reason from the IS
            FailureMessage = "Exception: " + GetExplanation(xmlMessage);
            FailureMessage += ". The action couldn't be performed because of the following reason: " + GetTheReason(xmlMessage);

        }

        private string GetTheReason(string xmlMessage)
        {
            XPathNavigator nav;
            XPathDocument docNav;
            XPathNodeIterator NodeIterNamespace;
            XPathNodeIterator NodeIter;
            StringReader xmlInput = new StringReader(xmlMessage);
            string xslExpression = "description";
            string theReason = "";

            //XPath Document is a XMLDoc but optimized to be queried with Xpath/XSLT
            docNav = new XPathDocument(xmlInput);

            // Create a navigator to query with XPath.
            nav = docNav.CreateNavigator();
            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(nav.NameTable);

            // first, all the namespaces used in the document have to be retrieved and added to the namespace manager
            // the following query is to retrieve all the namespaces used in the document
            string XpathNamespace = "/*/namespace::*";
            try
            {
                System.Xml.XPath.XPathExpression XPathExprNamespace = XPathExpression.Compile(XpathNamespace, xmlnsManager);
                NodeIterNamespace = nav.Select(XPathExprNamespace);
                while (NodeIterNamespace.MoveNext())
                {
                    string namespace_uri = NodeIterNamespace.Current.Value;
                    string prefix = NodeIterNamespace.Current.LocalName;
                    xmlnsManager.AddNamespace(prefix, namespace_uri);
                };
            }
            catch (System.Exception e)
            {
                LogManager.Instance.Log("Error: error with the XPath query (" + xslExpression + ")" + e);
            }


            // second, the real query can be performed
            try
            {
                System.Xml.XPath.XPathExpression XPathExprQuery = XPathExpression.Compile(xslExpression, xmlnsManager);
                NodeIter = nav.Select(XPathExprQuery);

                if (NodeIter.Count == 1)
                {
                    NodeIter.MoveNext();
                    // access to the reason of the XML
                    theReason = NodeIter.Current.Value;
                }
                else
                {
                    theReason = NodeIter.Current.Value;
                }

            }
            catch (System.ArgumentException ex)
            {
                LogManager.Instance.Log("The XPath expression parameter is not a valid XPath expression (" + xslExpression + ")" + ex);
            }
            catch (System.Xml.XPath.XPathException ex)
            {
                LogManager.Instance.Log("The XPath expression is not valid (" + xslExpression + ")" + ex);
            }
            catch (System.Exception ex)
            {
                LogManager.Instance.Log("Error: error with the XPath query (" + xslExpression + ")" + ex);
            }

            return theReason;
        }

        private string GetExplanation(string xmlMessage)
        {
            XPathNavigator nav;
            XPathDocument docNav;
            XPathNodeIterator NodeIterNamespace;
            XPathNodeIterator NodeIter;
            StringReader xmlInput = new StringReader(xmlMessage);
            string xslExpression = "exception";
            string theExplanation = "";

            //XPath Document is a XMLDoc but optimized to be queried with Xpath/XSLT
            docNav = new XPathDocument(xmlInput);

            // Create a navigator to query with XPath.
            nav = docNav.CreateNavigator();
            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(nav.NameTable);

            // first, all the namespaces used in the document have to be retrieved and added to the namespace manager
            // the following query is to retrieve all the namespaces used in the document
            string XpathNamespace = "/*/namespace::*";
            try
            {
                System.Xml.XPath.XPathExpression XPathExprNamespace = XPathExpression.Compile(XpathNamespace, xmlnsManager);
                NodeIterNamespace = nav.Select(XPathExprNamespace);
                while (NodeIterNamespace.MoveNext())
                {
                    string namespace_uri = NodeIterNamespace.Current.Value;
                    string prefix = NodeIterNamespace.Current.LocalName;
                    xmlnsManager.AddNamespace(prefix, namespace_uri);
                };
            }
            catch (System.Exception e)
            {
                LogManager.Instance.Log("Error: error with the XPath query (" + xslExpression + ")" + e);
            }


            // second, the real query can be performed
            try
            {
                System.Xml.XPath.XPathExpression XPathExprQuery = XPathExpression.Compile(xslExpression, xmlnsManager);
                NodeIter = nav.Select(XPathExprQuery);

                if (NodeIter.Count == 1)
                {
                    NodeIter.MoveNext();
                    // access to the reason of the XML
                    theExplanation = NodeIter.Current.Value;
                }
                else
                {
                    theExplanation = NodeIter.Current.Value;
                }

            }
            catch (System.ArgumentException ex)
            {
                LogManager.Instance.Log("The XPath expression parameter is not a valid XPath expression (" + xslExpression + ")" + ex);
            }
            catch (System.Xml.XPath.XPathException ex)
            {
                LogManager.Instance.Log("The XPath expression is not valid (" + xslExpression + ")" + ex);
            }
            catch (System.Exception ex)
            {
                LogManager.Instance.Log("Error: error with the XPath query (" + xslExpression + ")" + ex);
            }

            return theExplanation;
        }


        #region ILoadFailureReport Members

        public string FailureMessage { get; set; }

        public Exception FailureException { get; set; }

        #endregion
    }


}
