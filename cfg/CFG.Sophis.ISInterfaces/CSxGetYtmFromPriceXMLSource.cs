using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using sophis.reporting;
using sophis.utils;
using sophisTools;

namespace CFG.Sophis.ISInterfaces
{
    public class CSxGetYtmFromPriceXMLSource : CSMXMLSource
    {
        [Persistent]
        public string Date; //Date at format DD/MM/YYYY

        [Persistent]
        public string InstrumentReference;

        [Persistent]
        public int PricingModel;

        [Persistent]
        public int QuotationType;

        [Persistent]
        public double Price;

        /// <summary>
        /// GenerateXMLDescription is done to fill the dataSet that will be the output of the method
        /// </summary>
        /// <param name="dataSet">output of the method</param>
        /// <param name="inPreviewMode">true if we are in preview mode</param>
        /// <param name="generationType">specify xml, xsd, or any other transformation</param>
        public override void GenerateXMLDescription(System.Data.DataSet dataSet, bool inPreviewMode, eMGenerationType generationType)
        {
            CultureInfo currentCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;

            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            XElement xOutputElement = null;

            try
            {
                double ytm = 0;

                //Check date format
                CultureInfo frFR = new CultureInfo("fr-FR");
                DateTime dateTime;

                if (DateTime.TryParseExact(Date, "dd/MM/yyyy", frFR, DateTimeStyles.None, out dateTime) == false)
                {
                    throw new Exception("Invalid parameter date (" + Date + "). Format of date should be DD/MM/YYYY");
                }

                CSMDay dateObj = new CSMDay(dateTime.Day, dateTime.Month, dateTime.Year);
                int computationDate = dateObj.toLong();

                CSxBondPricerCLI bondPricer = new CSxBondPricerCLI();

                ytm = bondPricer.GetYTMFromPrice(InstrumentReference, computationDate, PricingModel, QuotationType, Price);


                xOutputElement = new XElement("MessageAcceped");

                XElement xResultsElement = new XElement("Results");
                xResultsElement.Add(new XElement("ytm", ytm));

                xOutputElement.Add(xResultsElement);
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

            //Restore culture
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCultureInfo;
        }
    }
}
