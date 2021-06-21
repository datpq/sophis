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
    public class CSxComputeTheoreticalXMLSource : CSMXMLSource
    {
        [Persistent]
        public string Date; //Date at format DD/MM/YYYY

        [Persistent]
        public string InstrumentReference;        

        [Persistent]
        public string CurveFamily;

        [Persistent]
        public int PricingModel;

        [Persistent]
        public int QuotationType;

        [Persistent]
        public double Spread; //CFG tkt field spread

        [Persistent]
        public string Maturities; //list of maturity. e.g. : 1d;3m;6m;...;2y;5y;...;30y

        [Persistent]
        public string Rates; //list of rates. e.g. : 2.3;2.5;3.6;...;5.5        

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
                double price = 0;
                double ytm = 0;
                double accruedCoupon = 0;
                double duration = 0;
                double sensitivity = 0;

                //Check date format
                CultureInfo frFR = new CultureInfo("fr-FR");
                DateTime dateTime;

                if (DateTime.TryParseExact(Date, "dd/MM/yyyy", frFR, DateTimeStyles.None, out dateTime) == false)
                {
                    throw new Exception("Invalid parameter date (" + Date + "). Format of date should be DD/MM/YYYY");
                }

                CSMDay dateObj = new CSMDay(dateTime.Day, dateTime.Month, dateTime.Year);
                int computationDate = dateObj.toLong();

                //Get maturities list
                List<string> maturitiesList = new List<string>();

                string[] MaturitiesSplit = Maturities.Split(new char[] { ';' });
                foreach (string s in MaturitiesSplit)
                {
                    if (s.Trim() != "")
                    {
                        maturitiesList.Add(s.Trim());
                    }
                }

                //Get rates list
                List<double> ratesList = new List<double>();

                string[] ratesSplit = Rates.Split(new char[] { ';' });
                foreach (string s in ratesSplit)
                {
                    if (s.Trim() != "")
                    {
                        double oneRate = 0;
                        if (double.TryParse(s, out oneRate))
                        {
                            ratesList.Add(oneRate);
                        }
                        else
                        {
                            throw new Exception("The list of rates (" + Rates + ") is invalid");
                        }
                    }
                }

                if (maturitiesList.Count != ratesList.Count)
                {
                    throw new Exception("The number of maturities (" + maturitiesList.Count.ToString() + ") does not match the number of rates (" +
                                            ratesList.Count.ToString() + ").\nList of maturitues is : " + Maturities + "\nList of rates is : " + Rates);
                }


                CSxBondPricerCLI bondPricer = new CSxBondPricerCLI();

                bondPricer.ComputeTheoretical(InstrumentReference, computationDate, CurveFamily, PricingModel, QuotationType, Spread, maturitiesList,
                                                ratesList, ref price, ref ytm, ref accruedCoupon, ref duration, ref sensitivity);


                xOutputElement = new XElement("MessageAcceped");

                XElement xResultsElement = new XElement("Results");
                xResultsElement.Add(new XElement("price", price));
                xResultsElement.Add(new XElement("ytm", ytm));
                xResultsElement.Add(new XElement("accruedCoupon", accruedCoupon));
                xResultsElement.Add(new XElement("duration", duration));
                xResultsElement.Add(new XElement("sensitivity", sensitivity));

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
