using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SophisETL.Common;
using SophisETL.Engine.IntegrationService.WebApi;
//using SophisETL.Engine.IntegrationService;
//using SophisETL.Engine.IntegrationService.Sophis.SOA.MethodDesigner;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using Sophis.Web.Api;
using SophisETL.Common.Logger;
using SophisETL.Extract.SOAExtract.Xml;


namespace SophisETL.Extract.SOAExtract
{
    //public class NS
    //{
    //    static public XNamespace fpml = "http://www.fpml.org/2005/FpML-4-2";
    //    static public XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
    //    static public XNamespace nsDataExchange = "http://sophis.net/sophis/gxml/dataExchange";
    //    static public XNamespace nsCommon = "http://www.sophis.net/common";
    //    static public XNamespace nsCommon2 = "http://sophis.net/sophis/common";
    //    static public XNamespace nsR = "http://www.sophis.net/reporting";
    //    static public XNamespace nsNetR = "http://www.sophis.net/DotNetReporting";
    //    static public XNamespace nsInstrument = "http://www.sophis.net/Instrument";
    //    static public XNamespace nsFund = "http://www.sophis.net/fund";
    //    static public XNamespace nsValuation = "http://www.sophis.net/valuation";
    //    static public XNamespace nsDividend = "http://www.sophis.net/dividend";
    //    static public XNamespace nsVolatility = "http://www.sophis.net/volatility";
    //    static public XNamespace nsTrade = "http://www.sophis.net/trade";
    //    static public XNamespace nsReporting = "http://www.sophis.net/reporting";
    //    static public XNamespace nsFolio = "http://www.sophis.net/folio";
    //}


	
    [SettingsType(typeof(Settings), "_Settings")]
    public class SOAExtract : IExtractStep
    {
        private Settings _Settings { get; set;}
        private int _RecordsExtractedCount = 0;
        private string logClassName = null;
        //private SoaMethodsRequest _SoaMethodsRequest;


       #region MethodDesignerExtract Chain Parameters
        public string Name { get; set; }
        // Only an Output Queue exists
        public IETLQueue TargetQueue { get; set; }
        public IETLQueue SourceQueue { get; set; }
        #endregion


        private IntegrationServiceEngine _ISEngine;


        public void Init()
        {
            logClassName = "[Transform/SOA/" + this.Name + "]";
            LogManager.Instance.Log(logClassName + " Starting Step...");
            // Request the initialization of the Integration Service Engine and Test that it works
            _ISEngine = IntegrationServiceEngine.Instance;
            //_ISEngine.CheckMethodDesignerService();            

            // Add a callback to our own NoMoreRecords event to dispose the Service
            //this.NoMoreRecords += new EventHandler(AddBenchmarkComposition_NoMoreRecords);
        }

        
        // Start of our thread, we load and push
        public event EventHandler NoMoreRecords;

        private bool IsRejected(string xmlResponse)
        {
            return xmlResponse.Contains("elementRejected");
        }

        public void Start()
        {
            int count = _Settings.soaParameters.Length;
            List<SoaMethodParameter> paramList = new List<SoaMethodParameter>(count);
            for (int i = 0; i < count; i++)
            {
                SoaMethodParameter param = new SoaMethodParameter();
                //param.Name = _Settings.soaParameters[i].soaParamName;
                param.Name = _Settings.soaParameters[i].soaParamName;
                param.Value = _Settings.soaParameters[i].soaParamValue;
                paramList.Add(param);
            }

            LogManager.Instance.Log(logClassName + " try to execute method:" + _Settings.soaMethod + " with parameters:" + paramList);
            XDocument resultXdoc = _ISEngine.ExecuteMethod(_Settings.soaMethod, paramList);
            if (resultXdoc != null)
            {
                string xmlMessage = resultXdoc.ToString();
                try
                {                    
                    if (IsRejected(xmlMessage))
                    {
                        LogManager.Instance.Log((new XmlISExtractFailureReport(xmlMessage)).FailureMessage);
                        NoMoreRecords(this, null);
                        return;
                    }
                    foreach (XElement node in ((resultXdoc.Root.Element(NS.nsR + "default") as XElement).Elements(NS.nsR + "defaultResult")))
                    {
                        Record record = Record.NewRecord(++_RecordsExtractedCount);
                        foreach (XNode e in node.Nodes())
                        {
                            XElement elt = e as XElement;
                            string fieldName = elt.Name.LocalName;
                            string fieldValue = elt.Value.ToString();
                            if (string.IsNullOrEmpty(fieldValue))
                            {
                                record.Fields.Add(fieldName, "");
                                record.Fields.Add(fieldName + "_IsNull", true);
                            }
                            else
                            {
                                record.Fields.Add(fieldName, fieldValue);
                                record.Fields.Add(fieldName + "_IsNull", false);
                            }

                        }
                        TargetQueue.Enqueue(record);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Log(ex.Message + " IsMessage:" + (new XmlISExtractFailureReport(xmlMessage)).FailureMessage);
                }
            }

            NoMoreRecords(this, null);
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
