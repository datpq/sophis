using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.reporting;
using System.Xml.Linq;
using sophis.utils;
using sophis.misc;
using Oracle.DataAccess.Client;
using Sophis.DataAccess;

namespace CFG_CorporateDataXMLSource
{
    public class CSxCorporateDataExportSource : CSMXMLSource
    {
        [Persistent]
        public string Columns;
        
        /// <summary>
        /// GenerateXMLDescription is done to fill the dataSet that will be the output of the method
        /// </summary>
        /// <param name="dataSet">output of the method</param>
        /// <param name="inPreviewMode">true if we are in preview mode</param>
        /// <param name="generationType">specify xml, xsd, or any other transformation</param>
        public override void GenerateXMLDescription(System.Data.DataSet dataSet, bool inPreviewMode, eMGenerationType generationType)
        {
            XDocument xDoc = GetXml();
            dataSet.ReadXml(xDoc.CreateReader());

        }

        /// <summary>
        /// Sample on how to fill a dataSet with XDocument
        /// </summary>
        /// <returns></returns>
        private XDocument GetXml()
        {   
            XElement xOutputElement = null;
            
            try
            {                
                CSMUserRights myUserRights = new CSMUserRights();
                CSMUserRights myGroupRights = new CSMUserRights(Convert.ToUInt32(myUserRights.GetParentID()));
                myUserRights.LoadDetails();
                myGroupRights.LoadDetails();
                eMRightStatusType ExpectedData = myUserRights.GetUserDefRight("Expected Data Access");
                eMRightStatusType ExpectedDataGroup = myGroupRights.GetUserDefRight("Expected Data Access");
                eMRightStatusType OfficialData = myUserRights.GetUserDefRight("Official Data Access");
                eMRightStatusType OfficialDataGroup = myGroupRights.GetUserDefRight("Official Data Access");
                int UserIdent = myUserRights.GetIdent();

                bool hasExpectedRight = false;
                bool hasOfficialRight = false;

                string SQLQuery = "select " + Columns + " from CFG_DONNEES_CORPORATES where Nature_de_donnees in (";
                if ((ExpectedData == eMRightStatusType.M_rsReadOnly) || (ExpectedData == eMRightStatusType.M_rsReadWrite) || (UserIdent == 1) || ((ExpectedData == eMRightStatusType.M_rsSameAsParent) && (ExpectedDataGroup == eMRightStatusType.M_rsReadOnly)) || ((ExpectedData == eMRightStatusType.M_rsSameAsParent) && (ExpectedDataGroup == eMRightStatusType.M_rsReadWrite)))
                {
                    SQLQuery += "'Expected'";
                    hasExpectedRight = true;
                }
                if ((OfficialData == eMRightStatusType.M_rsReadOnly) || (OfficialData == eMRightStatusType.M_rsReadWrite) || (UserIdent == 1) || ((OfficialData == eMRightStatusType.M_rsSameAsParent) && (OfficialDataGroup == eMRightStatusType.M_rsReadOnly)) || ((OfficialData == eMRightStatusType.M_rsSameAsParent) && (OfficialDataGroup == eMRightStatusType.M_rsReadWrite)))
                {
                    if (hasExpectedRight)
                        SQLQuery += ",";
                    SQLQuery += "'Published'";
                    hasOfficialRight = true;
                }
                SQLQuery += ")";

                XElement xCultureElement = new XElement("ServerCulture", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
                xOutputElement = new XElement("MessageAcceped", xCultureElement);
                
                XElement xResultsElement = new XElement("Results");

                if (hasExpectedRight || hasOfficialRight)
                {
                    OracleCommand myCommand = new OracleCommand(SQLQuery, DBContext.Connection);

                    OracleDataReader myReader = myCommand.ExecuteReader();                    

                    while (myReader.Read())
                    {
                        XElement xElem = new XElement("Result");

                        for (int i = 0; i < myReader.FieldCount; i++)
                        {
                            xElem.Add(new XElement(myReader.GetName(i), myReader.GetValue(i).ToString()));
                        }

                        xResultsElement.Add(xElem);
                    }

                    myCommand.Dispose();
                }

                xOutputElement.Add(xResultsElement);
            }
            catch (Exception ex)
            {
                xOutputElement = new XElement("MessageRejected", new XElement("Exception",ex.Message));
            }            
         
            XDocument xdoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "no"),
                //Important
                //The first Node of the xml MUST be called "DUMMY"
                new XElement("DUMMY", xOutputElement)
               );                      
                        
            return xdoc;
        }
    }
}
