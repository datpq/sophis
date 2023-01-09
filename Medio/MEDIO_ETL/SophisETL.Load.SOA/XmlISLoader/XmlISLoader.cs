using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.XPath;


using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.ErrorMgt;
using SophisETL.Queue;

using SophisETL.Load.XmlISLoader.Xml;
using System.Xml.Linq;
using SophisETL.ISEngine;




namespace SophisETL.Load.XmlISLoader
{
    [SettingsType(typeof(Settings), "_Settings")]
    class XmlISLoader : ILoadStep
    {
        // Injectable Dependency
        public string Name { get; set; }
        public IETLQueue SourceQueue { get; set; }

        // Members
        private Settings _Settings { get; set; }
        private int _RecordsProcessedCount = 0;
        private IntegrationServiceEngine _ISEngine;
      


        public void Init()
        {
            // Request the initialization of the Integration Service Engine
            _ISEngine = IntegrationServiceEngine.Instance;
        }

        public void Start() // start in its own thread
        {
            LogManager.Instance.Log( "Load/XmlIS/" + Name + ": starting step" );

            while ( SourceQueue.HasMore )
            {
                Record record = SourceQueue.Dequeue();
                if ( record != null )
                    Load( record );
            }

            LogManager.Instance.Log( "Load/XmlIS/" + Name + ": step finished - "
                    + _RecordsProcessedCount + " record(s) loaded");
        }

        private void Load( Record record )
        {
            if ( !record.Fields.ContainsKey( _Settings.sourceField ) )
            {

                LogManager.Instance.Log( "Load/XmlIS/" + Name + ": ERROR - record does not contain source XML field " + _Settings.sourceField + ", discarding" );
                return;
            }

            // Get the XML to pass
            string xmlMessage = Convert.ToString( record.Fields[_Settings.sourceField] );
            LogManager.Instance.LogDebug( String.Format( "[Load/XmlISLoader/{0}] Record[{1}] - Integration Service Request:{2}", Name, record.Key, xmlMessage ) );

            //string xmlResponse = _ISEngine.DataExchangeProcessRaw( xmlMessage ) ?? "<null reply>";
            string xmlResponse = (_ISEngine.Import(xmlMessage) ?? new XDocument()).ToString();
            LogManager.Instance.LogDebug( String.Format( "[Load/XmlISLoader/{0}] Record[{1}] - Integration Service Reply:{2}", Name, record.Key, xmlResponse ) );

            if ( IsSuccess( xmlResponse ) )
            {
                if ( RecordLoaded != null )
                    RecordLoaded( this, record, new XmlISLoadSuccessReport( xmlResponse ) );
            }
            else
            {
                if ( RecordNotLoaded != null )
                    RecordNotLoaded( this, record, new XmlISLoadFailureReport( xmlResponse ) );
            }

            _RecordsProcessedCount++;
        }

        private bool IsSuccess( string xmlResponse )
        {
            return ( xmlResponse.Contains( "ImportMessageAccepted" ) ||
                xmlResponse.Contains( "ImportMarketDataMessageAccepted") );
        }

        public event RecordLoadedEventHandler RecordLoaded;
        public event RecordNotLoadedEventHandler RecordNotLoaded;
    }

    class XmlISLoadSuccessReport : ILoadSuccessReport
    {
        public XmlISLoadSuccessReport( string xmlMessage )
        {
            SuccessMessage = "The action has been performed correctly.";
        }

        #region ILoadSuccessReport Members

        public string SuccessMessage { get; set; }

        #endregion
    }


    class XmlISLoadFailureReport : ILoadFailureReport
    {
        public XmlISLoadFailureReport( string xmlMessage )
        {
            // Default values
            FailureMessage = "Unknown Error";

            // parse the XML to get the reason from the IS
            string    reason;
            Exception exception;
            if ( GetErrorDetails(xmlMessage, out reason, out exception) )
            {
                FailureMessage   = reason ?? FailureMessage;
                FailureException = exception; // can be null
            }
        }


        /// <summary>
        /// Parse the Integration Service to look for the error
        /// There is two key pieces of information here:
        /// - elementRejected\reason\description : general message
        /// - elementRejected\details\exception  : detailed message
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <returns>true if parsing of (at least) reason was successful</returns>
        private bool GetErrorDetails(string xmlMessage, out string reason, out Exception exception)
        {
            XPathNavigator nav;
            XPathDocument docNav;
            XPathNodeIterator NodeIterNamespace;
            StringReader xmlInput = new StringReader(xmlMessage);
            string xslReasonExpression    = "//ns1:description";
            string xslExceptionExpression = "//ns0:exception";
            string xslRefExceptionExpression = "//ns4:reference";

            // default values
            reason    = null;
            exception = null;

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
                LogManager.Instance.Log("IS XML Reply Parsing Error: failed to read namespaces: " + e);
                return false;
            }

            // success is all dependent of reason extraction
            bool success = GetNodeValueByXPath(nav, xmlnsManager, xslReasonExpression, ref reason);
            if ( success )
            {
                // try to get exception as "cherry on the cake"
                string exceptionString = "";
                if (GetNodeValueByXPath(nav, xmlnsManager, xslExceptionExpression, ref exceptionString))
                {  
                    // try to get the ref 
                    string refMissingReason = "";
                    success = GetNodeValueByXPath(nav, xmlnsManager, xslRefExceptionExpression, ref refMissingReason);
                    if (success)
                    {
                        exceptionString += " : " + refMissingReason;
                    }
                    exception = new Exception(exceptionString);
                }
            }

            return success;
        }

        private static bool GetNodeValueByXPath(XPathNavigator nav, XmlNamespaceManager xmlnsManager, string expression, ref string value)
        {
            XPathNodeIterator NodeIter;

            // second, the real query can be performed
            try
            {
                System.Xml.XPath.XPathExpression XPathExprQuery = XPathExpression.Compile(expression, xmlnsManager);
                NodeIter = nav.Select(XPathExprQuery);

                if (NodeIter.Count > 0)
                {
                    NodeIter.MoveNext();
                    // access to the reason of the XML
                    value = NodeIter.Current.Value;

                    return true;
                }

            }
            catch (System.ArgumentException ex)
            {
                LogManager.Instance.Log("The XPath expression parameter is not a valid XPath expression (" + expression + ")" + ex);
            }
            catch (System.Xml.XPath.XPathException ex)
            {
                LogManager.Instance.Log("The XPath expression is not valid (" + expression + ")" + ex);
            }
            catch (System.Exception ex)
            {
                LogManager.Instance.Log("Error: error with the XPath query (" + expression + ")" + ex);
            }

            return false;
        } 

        #region ILoadFailureReport Members

        public string FailureMessage { get; set; }

        public Exception FailureException { get; set; }

        #endregion
    }
}
