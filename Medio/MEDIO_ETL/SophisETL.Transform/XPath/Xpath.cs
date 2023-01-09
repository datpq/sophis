using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
// using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.Tools;
using SophisETL.Queue;
using SophisETL.Transform;

using SophisETL.Transform.XPath.Xml;


namespace SophisETL.Transform.XPath
{
    [SettingsType(typeof(Settings), "_Settings")]
    public class XPath : AbstractBasicTransformTemplate
    {
        private Settings _Settings { get; set; }

        public override void Init()
        {
            base.Init();

            // basic check
            if ( _Settings.sourceField == null || _Settings.sourceField.Length == 0 ) { throw new Exception( "The source field in the configuration file is empty." ); }
            if ( _Settings.queryXPath == null  || _Settings.queryXPath.Length == 0  ) { throw new Exception("The XPath query in the configuration file is empty."); }
            if ( _Settings.targetField == null || _Settings.targetField.Length == 0 ) { throw new Exception("The target field in the configuration file is empty."); }
        }

        protected override Record Transform( Record record )
        {
            String XMLString    = _Settings.sourceField;
            String XPathString  = _Settings.queryXPath;
            String fieldResult  = _Settings.targetField;

            if (!record.Fields.ContainsKey(XMLString))
            {
                throw new Exception("Source Field " + XMLString + " not found in current record");
            }
            else
            {
                string plainXML = (string)record.Fields[XMLString];
                // log.Log("type of the plainXML object: " + plainXML.GetType());

                object theResult = TransformXml(plainXML, XPathString);

                LogManager.Instance.LogDebug("value of the result object: " + theResult);
                // Store the result field
                record.SafeAdd( fieldResult, theResult, eExistsAction.replace );
                // Store the Count flag
                int resultCount;
                if ( theResult == null )
                    resultCount = 0;
                else if ( theResult is System.Collections.ICollection )
                    resultCount = ( theResult as System.Collections.ICollection ).Count;
                else
                    resultCount = 1;
                record.SafeAdd( fieldResult + "_Count", resultCount, eExistsAction.replace );
            }
            
            return record;
        }

         public object TransformXml(string xml, string xslExpression)
         {
             XPathNavigator     nav;
             XPathDocument      docNav;
             XPathNodeIterator  NodeIterNamespace;
             XPathNodeIterator  NodeIter;
             StringReader       xmlInput = new StringReader(xml);
             object             theResult = null;

             //XPath Document is a XMLDoc but optimized to be queried with Xpath/XSLT
             docNav = new XPathDocument(xmlInput);

             // Create a navigator to query with XPath.
             nav = docNav.CreateNavigator();

             XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(nav.NameTable);
             
             // first, all the namespaces used in the document have to be retrieved and added to the namespace manager
             // the following query is to retrieve all the namespaces used in the document
             string XpathNamespace = "/*/namespace::*";
             System.Xml.XPath.XPathExpression XPathExprNamespace = XPathExpression.Compile(XpathNamespace, xmlnsManager);
             NodeIterNamespace = nav.Select(XPathExprNamespace);
             while (NodeIterNamespace.MoveNext())
             {
                 // stores the namespace-uri
                 string namespace_uri  = NodeIterNamespace.Current.Value;
                 
                 // stores the prefix
                 string prefix         = NodeIterNamespace.Current.LocalName;
                 xmlnsManager.AddNamespace(prefix, namespace_uri);
             }
             // Also Add the namespaces defined by the user in the setup file
             foreach( Namespace nmespace in _Settings.namespaces )
                 xmlnsManager.AddNamespace( nmespace.prefix, nmespace.Value );

             // second, the real XPath query can be performed
             try
             {
                 System.Xml.XPath.XPathExpression XPathExprQuery = XPathExpression.Compile(xslExpression, xmlnsManager);
                 NodeIter = nav.Select(XPathExprQuery);

                 if ( NodeIter.Count == 0 )
                 {
                     // No Result Found, keep the field to null
                 }
                 else if (NodeIter.Count == 1)
                 {
                     NodeIter.MoveNext();
                     string resultFromQuery = NodeIter.Current.Value;

                     // check on the type of the result
                     double resultDouble = .0;
                     DateTime resultDate = new DateTime();

                     // log.Log("Result of the Xpath Query: " + resultFromQuery);
                     // log.Log("Type of the result of the Xpath Query: " + resultFromQuery.GetType());
                     if (IsNumeric(resultFromQuery, ref resultDouble))              
                     {
                         // log.Log("the result from query is a double: " + resultDouble);
                         theResult = resultDouble;
                     }
                     else if (DateTime.TryParse(resultFromQuery, out resultDate))   
                     {
                         theResult = resultDate;
                     }
                     else                                                           
                     {
                         // log.Log("the result from query is a string: " + resultFromQuery);
                         theResult = resultFromQuery; 
                     }
                     // log.Log("Value of casted result: " + theResult);
                     // log.Log("Type of casted result: " + theResult.GetType());
                 }
                 else
                 {
                     // creates a list of all the nodes retrieved by the XPath query
                     List<object> listNodes = new List<object>();
                     while (NodeIter.MoveNext())
                     {
                         if ( NodeIter.Current.Value != null && NodeIter.Current.Value.Length > 0 )
                             // We have an internal value, let's take it
                             listNodes.Add( NodeIter.Current.TypedValue );
                         else
                            // Add the outerXML (nodes + attributes + namespaces) in the list 
                             listNodes.Add( NodeIter.Current.OuterXml );
                         
                     };
                     theResult = listNodes;
                 }
             }
             catch (System.ArgumentException ex)
             {
                 throw new Exception("The XPath expression parameter is not a valid XPath expression (" + xslExpression + ")", ex);
             }
             catch (System.Xml.XPath.XPathException ex)
             {
                 throw new Exception("The XPath expression is not valid (" + xslExpression + ")", ex);
             }
             catch (System.Exception ex)
             {
                 throw new Exception("Error: error with the XPath query (" + xslExpression + ")", ex);
             }

             return theResult;
         }

         private bool IsNumeric(object Expression, ref double dValue)
         {
             bool bIsNum = false;
             double dRetNum = .0;
             try
             {
                 bIsNum = Double.TryParse(Convert.ToString(Expression),
                                         System.Globalization.NumberStyles.Any,
                                         System.Globalization.NumberFormatInfo.InvariantInfo,
                                         out dRetNum);
             }
             catch (System.Exception ex)
             {
                 throw new Exception("Error: error during the parsing of the value retrieved: " + ex);
             }

             dValue = dRetNum;
             return bIsNum;
         }
    }
}
