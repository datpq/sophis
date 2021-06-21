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
    public class CSxCorporateDataImportSource : CSMXMLSource
    {
        [Persistent]
        public string SQLQuery;

        [Persistent]
        public string NatureDonnees;

        /// <summary>
        /// GenerateXMLDescription is done to fill the dataSet that will be the output of the method
        /// </summary>
        /// <param name="dataSet">output of the method</param>
        /// <param name="inPreviewMode">true if we are in preview mode</param>
        /// <param name="generationType">specify xml, xsd, or any other transformation</param>
        public override void GenerateXMLDescription(System.Data.DataSet dataSet, bool inPreviewMode, eMGenerationType generationType)
        {
            //XDocument xDoc = GetXml();
            //dataSet.ReadXml(xDoc.CreateReader());
            
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

                if (NatureDonnees == "Expected")
                {
                    if ((ExpectedData == eMRightStatusType.M_rsReadWrite) || (UserIdent == 1) || ((ExpectedData == eMRightStatusType.M_rsSameAsParent) && (ExpectedDataGroup == eMRightStatusType.M_rsReadWrite)))
                    {
                        OracleCommand myCommand = new OracleCommand(SQLQuery, DBContext.Connection);
                        myCommand.ExecuteNonQuery();
                        myCommand.Dispose();
                    }
                    else
                    {
                        throw new Exception("This user does not have the right to create or modify 'Expected' data");                    
                    }
                }
                else if (NatureDonnees == "Published")
                {
                    if ((OfficialData == eMRightStatusType.M_rsReadWrite) || (UserIdent == 1) || ((OfficialData == eMRightStatusType.M_rsSameAsParent) && (OfficialDataGroup == eMRightStatusType.M_rsReadWrite)))
                    {
                        OracleCommand myCommand = new OracleCommand(SQLQuery, DBContext.Connection);
                        myCommand.ExecuteNonQuery();
                        myCommand.Dispose();
                    }
                    else
                    {
                        throw new Exception("This user does not have the right to create or modify 'Published' data");
                    }
                }
                else
                {
                    throw new Exception("'Nature de donnees' should be 'Expected' or 'Published'");                    
                }

                xOutputElement = new XElement("MessageAcceped");
            }
            catch (System.Exception ex)
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
