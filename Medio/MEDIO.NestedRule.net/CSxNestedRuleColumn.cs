using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NSREnums;
using sophis.gui;
using sophis.instrument;
using sophis.portfolio;
using sophis.utils;

namespace MEDIO.NestedRule.net
{


    public class CSxNestedRuleColumn : CSMPortfolioColumn
    {
        static double fDigit = 0.0001;
        public int nType = 1;//biggest issuer
        public int nLevel1Value = 35;
        public int nLevel1Count = 2;
        public int nLevel2Value = 60;
        public int nLevel2Count = 4;
        public int nLevel3Value = 90;
        public int nLevel3Count = 6;
        public int nLevel4Value = 100;

        // Cache to have folioCode - > Issuer name , Issuer Weight
        public static Dictionary<int, Dictionary<string,double>> dFolioIssuerWeight = new Dictionary<int,Dictionary<string, double>>();
       // publistc Dictionary<int, Dictionary<string, int>> dFolioWeightCount = new Dictionary<int, Dictionary<string, int>>();
        public static Dictionary<int,int> dFolioAssetCount = new Dictionary<int, int>();
        public static Dictionary<int, string> dFolioMaxIssuername = new Dictionary<int, string>();
        public static Dictionary<string,Dictionary<int,double>> dIssuerInstrumentWeight= new Dictionary<string,Dictionary<int,double>>();
        public static HashSet<int> hDistinctList = new HashSet<int>();
        public static int refreshVersion = -1;

        public bool activeCodeChecked = false;

        public void fillIssuerFolioCache(int portfolioCode, sophis.portfolio.CSMExtraction extraction, int activePortfolioCode)
        {
            //flagging for positionCell value to retrieve the weight...in case 5, we should see the "breach"
            int oldtype = nType;

            try
            {
               
                
                nType = 1;

                CSMPortfolio portfolio = CSMPortfolio.GetCSRPortfolio(portfolioCode, extraction);
                if (portfolio == null)
                    return;

                // TODO : Get Folio tree positions, all of them....
                int folioNbpositions = portfolio.GetTreeViewPositionCount();

                // Number of asset to be counted should be done once only
                int nbFlatPositions = portfolio.GetFlatViewPositionCount(); ;
                dFolioAssetCount[portfolioCode] = portfolio.GetFlatViewPositionCount();
                CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Adding nb of flat position : " + nbFlatPositions + " for portfolio: " + portfolioCode);
                //Removing closed flat view position if any...
                for (int p = 0; p < nbFlatPositions; p++)
                {
                    CSMPosition pos = portfolio.GetNthFlatViewPosition(p);
                    CSMInstrument ins = pos.GetCSRInstrument();
                    CMString insName = ins.GetName();

                    CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Total asset Flat : " + dFolioAssetCount[portfolioCode] + " Checking Position nb: " + p + "," + insName.ToString() + " with qty : " + pos.GetInstrumentCount());
                    if (Math.Abs(pos.GetInstrumentCount()) < Math.Pow(10,-6))
                    {
                        CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Removing Position on : " + insName.ToString() + " with qty : " + pos.GetInstrumentCount()); 
                        dFolioAssetCount[portfolioCode] = dFolioAssetCount[portfolioCode] - 1;
                    }
                }


                CSMPortfolioColumn IssuerColumn = CSMPortfolioColumn.GetCSRPortfolioColumn("Sector UltimateParentIssuer");
                //  CSMPortfolioColumn WeightColumn = CSMPortfolioColumn.GetCSRPortfolioColumn("Weight in Fund (RBC)");

                // fill in for all direct underneath positions...
                for (int i = 0; i < folioNbpositions; i++)
                {
                    CSMPosition pos = portfolio.GetNthTreeViewPosition(i);
                    if (pos != null)
                    {
                        // Getting issuer and weight for the position...
                        if (pos.GetInstrumentCount() != 0.0)
                        {
                            //open position, checking for issuer first :
                            if (IssuerColumn != null)
                            {
                                SSMCellValue cellValueTmp = new SSMCellValue();
                                SSMCellStyle cellStyleTmp = new SSMCellStyle();
                                int instrumentCode = pos.GetInstrumentCode();

                                IssuerColumn.GetPositionCell(pos, activePortfolioCode, portfolioCode, extraction, 0, instrumentCode, ref cellValueTmp, cellStyleTmp, true);
                                string sIssuer = cellValueTmp.GetString();

                               // if (sIssuer != null)
                                {
                                    if(string.IsNullOrEmpty(sIssuer))
                                            sIssuer ="N/A";
                                    //check for the weight...
                                    SSMCellValue wcellValueTmp = new SSMCellValue();
                                    SSMCellStyle wcellStyleTmp = new SSMCellStyle();
                         

                                    GetPositionCell(pos, activePortfolioCode, portfolioCode, extraction, instrumentCode, instrumentCode, ref wcellValueTmp, wcellStyleTmp, true);

                                    double weight = wcellValueTmp.doubleValue;

                                  //  if (Math.Abs(weight) > 0)
                                    {
                                  /*      hDistinctList.Add(instrumentCode);

                                        if (hDistinctList.Contains(instrumentCode) == false)
                                            if (dFolioAssetCount.ContainsKey(portfolioCode) == true)
                                                dFolioAssetCount[portfolioCode] += 1;
                                            else
                                                dFolioAssetCount.Add(portfolioCode, 1); */

                                        //check if entry for the folio already
                                        if (dFolioIssuerWeight.ContainsKey(portfolioCode) == true)
                                        {
                                            if (dFolioIssuerWeight[portfolioCode].ContainsKey(sIssuer) == true)
                                            {
                                                CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_verbose, "Adding weight: " + weight + " for portfolio: " + portfolioCode+ " and issuer: "+sIssuer );
                                                dFolioIssuerWeight[portfolioCode][sIssuer] += weight;
                                             
                                            }
                                            else
                                            {
                                                CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_verbose, "Adding weight: " + weight + " for portfolio: " + portfolioCode + " and issuer: " + sIssuer);
                                                dFolioIssuerWeight[portfolioCode].Add(sIssuer, weight);
                                            }
                                        }
                                        else
                                        {
                                            Dictionary<string, double> newEntry = new Dictionary<string, double>();
                                            dFolioIssuerWeight.Add(portfolioCode, newEntry);
                                            CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_verbose, "Adding Weight Count for portfolio :  " + portfolioCode.ToString() + "Issuer : " + sIssuer);
                                            dFolioIssuerWeight[portfolioCode].Add(sIssuer, weight);

                                        }
                                            // TODO separate weight by issuer / instrument

                                         if(dIssuerInstrumentWeight.ContainsKey(sIssuer))
                                         {
                                             if(dIssuerInstrumentWeight[sIssuer].ContainsKey(instrumentCode))
                                             {
                                                 dIssuerInstrumentWeight[sIssuer][instrumentCode] += weight;
                                             }
                                             else
                                             {
                                                 dIssuerInstrumentWeight[sIssuer].Add(instrumentCode,weight);
                                             }
                                         }
                                         else
                                         {
                                             
                                             dIssuerInstrumentWeight.Add(sIssuer, new Dictionary<int,double>());
                                             dIssuerInstrumentWeight[sIssuer].Add(instrumentCode,weight);
                                         }

                                    }

                                }
                            }

                        }
                    }

                }

                //Scanning any underlying folio & their positions...

                int nbSiblings = portfolio.GetSiblingCount();

                for (int j = 0; j < nbSiblings; j++)
                {
                    CSMPortfolio sibling = portfolio.GetNthSibling(j);

                    if (sibling != null)
                    {
                        //should fill the map for sibling folio
                        int siblingCode = sibling.GetCode();
                        fillIssuerFolioCache(siblingCode, extraction, activePortfolioCode);

                        //adding to the parent folio:


                        if (dFolioIssuerWeight.ContainsKey(siblingCode) == true)
                        {
                            foreach (string name in dFolioIssuerWeight[siblingCode].Keys)
                            {
                                if (dFolioIssuerWeight.ContainsKey(portfolioCode) == true)
                                {
                                    if (dFolioIssuerWeight[portfolioCode].ContainsKey(name))
                                        dFolioIssuerWeight[portfolioCode][name] += dFolioIssuerWeight[siblingCode][name];
                                    else
                                        dFolioIssuerWeight[portfolioCode].Add(name, dFolioIssuerWeight[siblingCode][name]);
                                }
                                else
                                {
                                    Dictionary<string, double> newEntry = new Dictionary<string, double>();
                                    dFolioIssuerWeight.Add(portfolioCode, newEntry);
                                    dFolioIssuerWeight[portfolioCode].Add(name, dFolioIssuerWeight[siblingCode][name]);

                                }
                            }

                        }

                        }

                    }

                nType = oldtype;
                
                }

            catch (Exception e)
            {
                nType = oldtype;
                CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Caught Exception : " + e.Message); 
            }

       }

        public void FillFolioMaxIssuerName()
        {
            try
            {
                foreach (int folioID in dFolioIssuerWeight.Keys)
                {
                    KeyValuePair<string, double> checkMax = dFolioIssuerWeight[folioID].First();
                    if(dFolioMaxIssuername.ContainsKey(folioID) == false)
                                dFolioMaxIssuername.Add(folioID, checkMax.Key);
                    foreach (KeyValuePair<string, double> max in dFolioIssuerWeight[folioID])
                    {
                        if (max.Value > checkMax.Value)
                        {
                            checkMax = max;
                            dFolioMaxIssuername[folioID] = max.Key;

                        }

                    }
                }

            }
            catch (Exception e)
            {
                string message = e.Message;
            }
        }

        public CSxNestedRuleColumn(int Type)
        {
            fGroup = "Toolkit";
            nType = Type;
        }

        double GetOneValue(string colName, CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode)
        {
            SSMCellValue cellValueTmp = new SSMCellValue();
            cellValueTmp.doubleValue = 0.0;

            SSMCellStyle cellStyleTmp = new SSMCellStyle();
            double value = 0.0;
            CSMPortfolioColumn column = CSMPortfolioColumn.GetCSRPortfolioColumn(colName);
            if (column != null)
            {
                column.GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref cellValueTmp, cellStyleTmp, true);
                value = cellValueTmp.doubleValue;
            }
            return value;
        }

        public override void GetPortfolioCell(int activePortfolioCode, int portfolioCode, sophis.portfolio.CSMExtraction extraction, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {

                if (CSMPortfolioColumn.GetRefreshVersion() != refreshVersion)
                {
                    CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Initializing Caches");

                    dFolioIssuerWeight.Clear();
                    dFolioAssetCount.Clear();
                    dFolioMaxIssuername.Clear();
                   // fillIssuerFolioCache(portfolioCode, extraction, activePortfolioCode);
                   // FillFolioMaxIssuerName();

                  //  CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Cache dFolioIssuerWeight" + dFolioIssuerWeight.Count);
                  //  CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Cache dFolioMaxIssuername:" + dFolioMaxIssuername.Count);
                  //  CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Cache dFolioWeightCount:" + dFolioAssetCount.Count);

                    refreshVersion = CSMPortfolioColumn.GetRefreshVersion();
                }

                if (dFolioIssuerWeight.Keys.Contains(activePortfolioCode) == false)
                {
                    fillIssuerFolioCache(activePortfolioCode, extraction, activePortfolioCode);
                    FillFolioMaxIssuerName();
                }

                string sMaxIssuer = "";
                double dMaxValue = 0.0;

                switch (nType)
                {
                  case 1://max issuer name
                        {
                            cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                            cellStyle.kind = NSREnums.eMDataType.M_dNullTerminatedString;
                            cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                            cellStyle.style = sophis.gui.eMTextStyleType.M_tsNormal;
                            sMaxIssuer = "";
                            if (dFolioMaxIssuername.ContainsKey(portfolioCode))
                            {
                                sMaxIssuer = dFolioMaxIssuername[portfolioCode];
                            }

                            cellValue.SetString(sMaxIssuer);
                        }
                        break;

                    case 2://max issuer weight
                        {
                            if (!onlyTheValue && cellStyle != null)
                            {
                                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                                cellStyle.@decimal = 2;
                                cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                                cellStyle.style = sophis.gui.eMTextStyleType.M_tsNormal;
                            }

                            sMaxIssuer = "";
                            dMaxValue = 0.0;

                            if (dFolioMaxIssuername.ContainsKey(portfolioCode))
                                sMaxIssuer = dFolioMaxIssuername[portfolioCode];

                            if (dFolioIssuerWeight.ContainsKey(portfolioCode))
                            {
                                if (string.IsNullOrEmpty(sMaxIssuer))
                                    sMaxIssuer = "N/A";
                                dMaxValue = dFolioIssuerWeight[portfolioCode][sMaxIssuer];

                                cellValue.doubleValue = dMaxValue;
                            }
                           
                        }
                        break;

                    case 3://max issuer count
                        {
                            if (!onlyTheValue && cellStyle != null)
                            {
                                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                                //cellStyle.kind = NSREnums.eMDataType.M_dInt;
                                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                                cellStyle.@decimal = 0;
                               // cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                                cellStyle.style = sophis.gui.eMTextStyleType.M_tsNormal;
                            }

                            sMaxIssuer = "";
                            int count = 0;
                            if (dFolioMaxIssuername.ContainsKey(portfolioCode))
                                sMaxIssuer = dFolioMaxIssuername[portfolioCode];

                            if (dFolioAssetCount.ContainsKey(portfolioCode) )//&& sMaxIssuer != "")
                                count = dFolioAssetCount[portfolioCode];

                           // cellValue.integerValue = count;
							cellValue.doubleValue = Convert.ToDouble(count);
                        }

                        break;

                    case 4://max issuer rule
                        {
                            if (!onlyTheValue && cellStyle != null)
                            {
                                // display value will be aligned to the right
                                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                                //cellStyle.kind = NSREnums.eMDataType.M_dInt;
                                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                                cellStyle.@decimal = 0;
                             //   cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                                cellStyle.style = sophis.gui.eMTextStyleType.M_tsNormal;
                            }

                            int nNestedRuleValue = 0;

                            int count = 0;

                            CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Trying to get dFolioMaxIssuername with portfolio Code " + portfolioCode.ToString());

                            if (dFolioMaxIssuername.ContainsKey(portfolioCode))
                                sMaxIssuer = dFolioMaxIssuername[portfolioCode];

                            CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Trying to get dFolioWeightCount with portfolio Code " + portfolioCode.ToString());
                            if (dFolioAssetCount.ContainsKey(portfolioCode) )// && sMaxIssuer != "")
                                count = dFolioAssetCount[portfolioCode];

                            dMaxValue = 0.0;

                            if (dFolioMaxIssuername.ContainsKey(portfolioCode))
                                sMaxIssuer = dFolioMaxIssuername[portfolioCode];

                            CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "Trying to get dFolioIssuerWeight with portfolio Code " + portfolioCode.ToString());
                            if (dFolioIssuerWeight.ContainsKey(portfolioCode))
                            {
                                if (string.IsNullOrEmpty(sMaxIssuer))
                                    sMaxIssuer = "N/A";
                                dMaxValue = dFolioIssuerWeight[portfolioCode][sMaxIssuer];
                            }


                            CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_debug, "dMaxValue : " + dMaxValue.ToString());
                            if (dMaxValue > nLevel4Value)
                            {
                                nNestedRuleValue = 1;
                            }
                            else
                            {
                                if (dMaxValue >= nLevel3Value)
                                {
                                    if (count < nLevel3Count)
                                    {
                                        nNestedRuleValue = 1;
                                    }
                                }
                                else
                                {
                                    if (dMaxValue >= nLevel2Value)
                                    {
                                        if (count < nLevel2Count)
                                        {
                                            nNestedRuleValue = 1;
                                        }
                                    }
                                    else
                                    {
                                        if (dMaxValue >= nLevel1Value)
                                        {
                                            if (count < nLevel1Count)
                                            {
                                                nNestedRuleValue = 1;
                                            }
                                        }
                                    }
                                }
                            }

                          
                           // cellValue.integerValue = nNestedRuleValue;
                            cellValue.doubleValue = Convert.ToDouble(nNestedRuleValue);
                        }
                        break;

                    case 5: // if instrument >= 30% and issuer >=35% then return 1.
                        { 
                                                        if (!onlyTheValue && cellStyle != null)
                            {
                                // display value will be aligned to the right
                                cellStyle.alignment = sophis.gui.eMAlignmentType.M_aRight;
                                //cellStyle.kind = NSREnums.eMDataType.M_dInt;
                                cellStyle.kind = NSREnums.eMDataType.M_dDouble;
                                cellStyle.@decimal = 0;
                             //   cellStyle.@null = sophis.gui.eMNullValueType.M_nvZeroAndUndefined;
                                cellStyle.style = sophis.gui.eMTextStyleType.M_tsNormal;
                            }

                            // Only displayed at the portfolio level:

                            double retval =0.0;

                            if (dFolioIssuerWeight.ContainsKey(portfolioCode))
                            {
                                foreach (KeyValuePair<string, double> issuerWeight in dFolioIssuerWeight[portfolioCode])
                                {
                                    if (dFolioIssuerWeight[portfolioCode].ContainsKey(issuerWeight.Key))
                                    {
                                        if (dFolioIssuerWeight[portfolioCode][issuerWeight.Key] >= 35)
                                        {
                                            if (dIssuerInstrumentWeight.ContainsKey(issuerWeight.Key))
                                            {
                                                foreach (KeyValuePair<int, double> instrumentWeight in dIssuerInstrumentWeight[issuerWeight.Key])
                                                {
                                                    if (dIssuerInstrumentWeight[issuerWeight.Key][instrumentWeight.Key] >= 30)
                                                        retval = 1.0;
                                                }
                                            }
                                        }

                                        
                                    }
                                }
                            }

                            cellValue.doubleValue = Convert.ToDouble(retval);
                        }
                        break;
                }

                
            }
            catch (Exception ex)
            {
                CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Exception Caught : " + ex);
            }

        }

        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {
                cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                cellStyle.kind = eMDataType.M_dDouble;
                cellStyle.style = eMTextStyleType.M_tsNormal;
                cellStyle.alignment = eMAlignmentType.M_aCenter;
                cellStyle.@decimal = 2;
                
               switch(nType)
               {
                   case 5:
                       {
                           double retval = 0.0;
                           if (dIssuerInstrumentWeight.Count == 0)
                           {
                               fillIssuerFolioCache(activePortfolioCode, extraction, activePortfolioCode);
                           }

                           // get the issuer column
                           CSMPortfolioColumn IssuerColumn = CSMPortfolioColumn.GetCSRPortfolioColumn("Sector UltimateParentIssuer");

                           if (IssuerColumn != null)
                           {
                               SSMCellValue cellValueTmp = new SSMCellValue();
                               SSMCellStyle cellStyleTmp = new SSMCellStyle();
                               IssuerColumn.GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, 0, instrumentCode, ref cellValueTmp, cellStyleTmp, true);
                               string sIssuer = cellValueTmp.GetString();

                               if (!string.IsNullOrEmpty(sIssuer))
                               {
                                   if (dIssuerInstrumentWeight.ContainsKey(sIssuer))
                                   {
                                       if (dFolioIssuerWeight.ContainsKey(portfolioCode))
                                       {
                                               if (dFolioIssuerWeight[portfolioCode].ContainsKey(sIssuer))
                                               {
                                                   if (dFolioIssuerWeight[portfolioCode][sIssuer] >= 35)
                                                   {
                                                       if (dIssuerInstrumentWeight.ContainsKey(sIssuer))
                                                       {
                                                          if (dIssuerInstrumentWeight[sIssuer][instrumentCode] >= 30)
                                                                   retval = 1.0;
                                                       }
                                                   }
                                               }
                                           }
                                     }

                                 }
                              }

                              cellValue.doubleValue = retval;
                           }                          
                       break;

                   default :
                       CSMPortfolioColumn WeightColumn = CSMPortfolioColumn.GetCSRPortfolioColumn("Weight in Fund (RBC)");
                       if (WeightColumn != null)
                       {
                           SSMCellValue cellValueTmp = new SSMCellValue();
                           SSMCellStyle cellStyleTmp = new SSMCellStyle();

                           WeightColumn.GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref cellValueTmp, cellStyleTmp, true);

                           cellValue.doubleValue = cellValueTmp.doubleValue;
                       }
                       break;
            }

                //
            }
            catch (Exception ex)
            {
                CSMLog.Write(GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name, CSMLog.eMVerbosity.M_error, "Exception Caught : " + ex);
            }

        }
    }
}
 