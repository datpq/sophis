using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.reporting;
using System.Xml.Linq;
//using System.Data.OracleClient;
using sophis.utils;
using sophis.misc;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;

namespace CFG_CorporateDataXMLSource
{
    public class CSxSOAServerCultureSource : CSMXMLSource
    {   
        /// <summary>
        /// GenerateXMLDescription is done to fill the dataSet that will be the output of the method
        /// </summary>
        /// <param name="dataSet">output of the method</param>
        /// <param name="inPreviewMode">true if we are in preview mode</param>
        /// <param name="generationType">specify xml, xsd, or any other transformation</param>
        public override void GenerateXMLDescription(System.Data.DataSet dataSet, bool inPreviewMode, eMGenerationType generationType)
        {            
            XElement xOutputElement = null;

            try
            {                
                XElement xCultureElement = new XElement("ServerCulture", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                xOutputElement = new XElement("MessageAcceped", xCultureElement);                
            }
            catch (Exception ex)
            {
                xOutputElement = new XElement("MessageRejected", new XElement("Exception", ex.Message));
            }

            XDocument xDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                //Important
                //The first Node of the xml MUST be called "DUMMY"
                new XElement("DUMMY", xOutputElement)
               );

            dataSet.ReadXml(xDoc.CreateReader());

        }        
    }
}
