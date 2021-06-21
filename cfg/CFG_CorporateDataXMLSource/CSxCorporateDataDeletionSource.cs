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
    public class CSxCorporateDataDeletionSource : CSMXMLSource
    {
        [Persistent]
        public string Emetteur;

        [Persistent]
        public int Annee;

        [Persistent]
        public string NatureDonnees;

        [Persistent]
        public string Source;

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
                string SQLQuery = "delete from CFG_DONNEES_CORPORATES where EMETTEUR = '" + Emetteur + "'" +
                                                    " AND ANNEE = " + Annee +
                                                    " AND NATURE_DE_DONNEES = '" + NatureDonnees + "'" +
                                                    " AND SOURCES = '" + Source + "'"; ;
                OracleCommand myCommand = new OracleCommand(SQLQuery, DBContext.Connection);
                myCommand.ExecuteNonQuery();
                myCommand.Dispose();

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
