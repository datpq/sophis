using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;
using System.Runtime.InteropServices;
using sophis.reporting;
using sophis.utils;
using sophis.instrument;
using sophisTools;
using sophis.static_data;
using sophis.market_data;

namespace CFG.Sophis.ISInterfaces
{
    public class CSxGetYieldCurveXMLSource : CSMXMLSource
    {
        [Persistent]
        public string Date; //Date at format DD/MM/YYYY

        [Persistent]
        public string Currency;

        [Persistent]
        public string CurveFamily;

        [Persistent]
        public string CurveName;   
        
       

        /// <summary>
        /// GenerateXMLDescription is done to fill the dataSet that will be the output of the method
        /// </summary>
        /// <param name="dataSet">output of the method</param>
        /// <param name="inPreviewMode">true if we are in preview mode</param>
        /// <param name="generationType">specify xml, xsd, or any other transformation</param>
        public override void GenerateXMLDescription(System.Data.DataSet dataSet, bool inPreviewMode, eMGenerationType generationType)
        {
            CSMLog _logger = new CSMLog();
            _logger.Begin("CSxGetYieldCurveXMLSource", "GenerateXMLDescription");

            CultureInfo currentCultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;

            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            XElement xOutputElement = null;

            try
            {                
                CSMMarketData marketData = CSMMarketData.GetCurrentMarketData();
                int currentDate = marketData.GetDate();

                int computationDate = currentDate;

                //Check date format

                if (Date != "")
                {
                    CultureInfo frFR = new CultureInfo("fr-FR");
                    DateTime dateTime;

                    if (DateTime.TryParseExact(Date, "dd/MM/yyyy", frFR, DateTimeStyles.None, out dateTime) == false)
                    {
                        throw new Exception("Invalid parameter date (" + Date + "). Format of date should be DD/MM/YYYY");
                    }

                    CSMDay dateObj = new CSMDay(dateTime.Day, dateTime.Month, dateTime.Year);
                    computationDate = dateObj.toLong();
                }
                
                //If computation date is not equal to current date, change prices date to computation date                
                if (computationDate != currentDate)
                {
                    CSxUtil.SetPricesDate(computationDate);
                }

                //Check currency
                int ccyCode = CSMCurrency.StringToCurrency(Currency);
                if (ccyCode == 0)
                {
                    throw new Exception("Invalid currency parameter (" + Currency + ")");
                }

                //Check yield curve family parameter
                int yieldCurveFamilyCode = CSMYieldCurveFamily.GetYieldCurveFamilyCode(ccyCode,CurveFamily);
		        CSMYieldCurveFamily yieldCurveFamily = CSMYieldCurveFamily.GetCSRYieldCurveFamily(yieldCurveFamilyCode);

		        if (yieldCurveFamily == null)
		        {
                    throw new Exception("Invalid yield curve family parameter (" + CurveFamily + ")");
		        }

                CSMYieldCurve yieldCurve = null;

                //Check yield curve name
                if (CurveName != "")
                {
                    int yieldCurveCode = CSMYieldCurve.LookUpYieldCurveId(yieldCurveFamilyCode, CurveName);
                    yieldCurve = CSMYieldCurve.GetCSRYieldCurve(yieldCurveCode);
                    
                    if (yieldCurve == null)
                    {
                        throw new Exception("Invalid yield curve name parameter (" + CurveName + ")");
                    }
                }
                else
                {
                    yieldCurve = CSMYieldCurve.GetInstanceByYieldCurveFamily(yieldCurveFamilyCode);

                    if (yieldCurve == null)
                    {
                        throw new Exception("Could not retrieve default yield curve for family (" + yieldCurveFamilyCode.ToString() + ")");
                    }
                }                

                CSMDay computationDateObj = new CSMDay(computationDate);
                DateTime computationDateTime = new DateTime(computationDateObj.fYear, computationDateObj.fMonth, computationDateObj.fDay);
                
                XElement xYieldCurveElement = new XElement("yieldCurve");
                xYieldCurveElement.Add(new XElement("date", computationDateTime.ToString("yyyy-MM-dd")));
                xYieldCurveElement.Add(new XElement("currency", Currency));
                xYieldCurveElement.Add(new XElement("family", CurveFamily)); 

                bool zeroCouponYieldCurve = false;
                if (CurveFamily.CompareTo("COURBE ZERO COUPON") == 0)
                {
                    zeroCouponYieldCurve = true;
                    _logger.Write(CSMLog.eMVerbosity.M_debug, String.Format("'COURBE ZERO COUPON' family => force long term"));
                }
               

                XElement xPointsElement = new XElement("points");

                SSMYieldCurve ssYieldCurve = yieldCurve.GetSSYieldCurve();

                if (ssYieldCurve != null)
                {
                    xYieldCurveElement.Add(new XElement("name", ssYieldCurve.fName));

                    //DPH
                    //int nbPoints = ssYieldCurve.fPointCount;
                    int nbPoints = ssYieldCurve.fPoints.fPointCount;
                    int zcArrayIndex = 0;
                    int zcArrayIndexSize = yieldCurve.GetPointCount();

                    for (int i = 0; i < nbPoints; i++)
                    {
                        //DPH
                        //SSMYieldPoint yieldPoint = ssYieldCurve.fPointList.GetNthElement(i);
                        SSMYieldPoint yieldPoint = ssYieldCurve.fPoints.fPointList.GetNthElement(i);

                        CSMInfoSup infoSup = yieldPoint.fInfoPtr;
                        if (infoSup != null && infoSup.fIsUsed == true)
                        {   
                            XElement xPointElement = new XElement("point");

                            if (zcArrayIndex >= zcArrayIndexSize)
                            {
                                throw new Exception("zcArrayIndex ( " + zcArrayIndex + ") is out of range(" + zcArrayIndexSize + ")");
                            }

                            int timeToMaturity = yieldCurve.GetMaturity(zcArrayIndex);
                            int maturity = computationDate + timeToMaturity;
                            CSMDay pointMaturityDay = new CSMDay(maturity);
                            DateTime pointMaturityDateTime = new DateTime(pointMaturityDay.fYear, pointMaturityDay.fMonth, pointMaturityDay.fDay);
                            xPointElement.Add(new XElement("maturity", pointMaturityDateTime.ToString("yyyy-MM-dd")));

                            //Compute ZC rate                            				                    
		                    eMDayCountBasisType dcb = CSMPreference.GetDayCountBasisType();
		                    eMYieldCalculationType yc = CSMPreference.GetYieldCalculationType();

	                        SSMMaturity mat_1y = new SSMMaturity();
	                        mat_1y.fMaturity = 1;
	                        mat_1y.fType = 'y';
                            CSMCalendar calendar = (CSMCalendar)CSMCurrency.GetCSRCurrency(ccyCode);
	                        int timeToMaturity_1y = SSMMaturity.GetDayCount(mat_1y, currentDate, calendar);

                            int baseRateId = 0;
                            if (timeToMaturity <= timeToMaturity_1y && !zeroCouponYieldCurve)
                            {
	                            baseRateId = ssYieldCurve.fShortTermRate;
                                _logger.Write(CSMLog.eMVerbosity.M_debug, String.Format("Short Term Rate {0} ({1} <= {2})", baseRateId, timeToMaturity, timeToMaturity_1y));
                            }
                            else
                            {
	                            baseRateId = ssYieldCurve.fLongTermRate;
                                _logger.Write(CSMLog.eMVerbosity.M_debug, String.Format("Long Term Rate {0} ({1} > {2})", baseRateId, timeToMaturity, timeToMaturity_1y));
                            }

                            CSMInterestRate currentInterestRate = CSMInterestRate.GetInstance(baseRateId);
                            if (currentInterestRate != null)
                            {
                                dcb = currentInterestRate.GetDayCountBasisType(); // eMDayCountBasisType.M_dcb_Actual_Actual_AFB;
	                            yc = currentInterestRate.GetYieldCalculationType();
                                _logger.Write(CSMLog.eMVerbosity.M_debug, String.Format("Day Count Basis {0}, Yield Calculation {1}", dcb, yc));
                            }
                            else
                            {
                                _logger.Write(CSMLog.eMVerbosity.M_debug, String.Format("Use default Day Count Basis and Yield Calculation"));
                            }

                            double cf = yieldCurve.CompoundFactor((double)timeToMaturity);
                            double dt = CSMDayCountBasis.GetCSRDayCountBasis(dcb).GetEquivalentYearCount(computationDate, maturity, new SSMDayCountCalculation());
	                        double zc = CSMYieldCalculation.GetCSRYieldCalculation(yc).GetRate(cf - 1.0, dt);

                            xPointElement.Add(new XElement("rate", Math.Round(zc*100, 12)));                            

                            xPointsElement.Add(xPointElement);

                            zcArrayIndex++;
                        }                                                                                                                        
                    }
                }

                xYieldCurveElement.Add(xPointsElement);
                xOutputElement = new XElement("MessageAcceped");
                xOutputElement.Add(xYieldCurveElement);

                //Do not forget to restore current date
                if (computationDate != currentDate)
                {
                    CSxUtil.SetPricesDate(currentDate);
                }
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
