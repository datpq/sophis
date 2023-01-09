using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.OrderAutomation.NET.Source.OrderCreationValidator;
using NSREnums;
using sophis.gui;
using sophis.instrument;
using sophis.oms;
using sophis.portfolio;
using sophis.utils;
using sophis.OrderGeneration.PortfolioColumn;
using sophis.OrderGeneration;
using sophis.OrderGeneration.DOB.Builders;

namespace MEDIO.OrderAutomation.NET.Source.Column
{


    public class CSxRebalancingConstraint : CSMPortfolioColumn
    {
        public CSxRebalancingConstraint()
        {
            fGroup = "Toolkit";
        }

        static double fDigit = 0.0001;

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
        public override void GetPositionCell(CSMPosition position, int activePortfolioCode, int portfolioCode, CSMExtraction extraction, int underlyingCode, int instrumentCode, ref SSMCellValue cellValue, SSMCellStyle cellStyle, bool onlyTheValue)
        {
            try
            {
                bool test = CSxOrderCreationMiscValidator.CheckBreachColumn();
                if (!DynamicOrderBuilder.Instance.IsSessionActive)
                {
                    fListOfBreachedInstFolio.Clear();
                    return;
                }

                //if( position.GetIdentifier() < 0)
                //{
                //    return;
                //}

                cellStyle.@null = eMNullValueType.M_nvNoNullValue;
                cellStyle.kind = eMDataType.M_dNullTerminatedString;
                cellStyle.style = eMTextStyleType.M_tsNormal;
                cellStyle.alignment = eMAlignmentType.M_aCenter;
                cellValue.SetString("");

                CSMPortfolioColumn orderExposureInPrecent = CSMPortfolioColumn.GetCSRPortfolioColumn("Order Exposure in %");
                if (orderExposureInPrecent != null)
                {
                    SSMCellValue cellValueTmp = new SSMCellValue();
                    SSMCellStyle cellStyleTmp = new SSMCellStyle();
                    orderExposureInPrecent.GetPositionCell(position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode, ref cellValueTmp, cellStyleTmp, true);
                    double orderExpo = cellValueTmp.doubleValue;
                    if (Math.Abs(orderExpo) > Double.Epsilon)
                    {
                        int posId = position.GetIdentifier();

                        double relativeTargetExpo = GetOneValue("Relative Target Exposure In %", position, activePortfolioCode, portfolioCode, extraction, underlyingCode, instrumentCode);

                        double initExpo = relativeTargetExpo - orderExpo;

                        CSMLog.Write("CSxRebalancingConstraint", "GetPositionCell", CSMLog.eMVerbosity.M_debug, "For position " + position.GetIdentifier() + " initExpo = " + initExpo + "; relativeTargetExpo = " + relativeTargetExpo + "; orderExpo = " + orderExpo);
                        
                        if( initExpo >  0)
                        {
                            if (orderExpo > 0) //Wrong direction
                            {
                                SetBreach(ref cellValue, cellStyle, position, "Breach : Order opposite to Benchmark" );
                            }
                            else if( relativeTargetExpo < - ( 0.5 + fDigit) ) //Too far
                            {
                                SetBreach(ref cellValue, cellStyle, position, "Breach : 0.5% away from Benchmark");
                            }
                            else
                            {
                                 RemoveBreach(position);
                            }
                        }
                        else //initExpo < 0
                        {
                            if( orderExpo < 0 ) //Wrong direction
                            {
                                SetBreach(ref cellValue, cellStyle, position, "Breach : Order opposite to Benchmark");
                            }
                            else if (relativeTargetExpo > 0.5 + fDigit) //Too far
                            {
                                SetBreach(ref cellValue, cellStyle, position, "Breach : 0.5% away from Benchmark");
                            }
                            else
                            {
                                 RemoveBreach(position);
                            }
                        }
                    }
                    else
                    {
                        RemoveBreach(position);
                    }
                }
            }
            catch(Exception ex)
            {
                CSMLog.Write("CSxRebalancingConstraint", "GetPositionCell", CSMLog.eMVerbosity.M_warning, "Error:" + ex);
            }

        }

        //string GetInitialExpoColumnName()
        //{
           
        //    return "Initial Exposure in %";
        //}

        //string GetBenchmarkWeightColumnName()
        //{
        //    if( DynamicOrderBuilder.Instance.IsSessionActive )
        //    {
        //        if( OrderBuilderDialog.CurrentSessionContext.RefLevelData.RefLevel ==sophis.modelPortfolio.eMReferenceLevel.M_refLvlStrategy)
        //        {
        //            return "Strategy Benchmark Weights";
        //        }
        //        else
        //        {
        //            return "Fund Benchmark Weights";
        //        }
        //    }
        //    return "Strategy Benchmark Weights";
        //}

        //string GetExpoColumnName()
        //{
        //   if( DynamicOrderBuilder.Instance.IsSessionActive )
        //   {
        //        if( OrderBuilderDialog.CurrentSessionContext.RefLevelData.RefLevel ==sophis.modelPortfolio.eMReferenceLevel.M_refLvlStrategy)
        //        {
        //            return "Weight In Strategy";
        //        }
        //        else
        //        {
        //            return "Weight In Fund";
        //        }
        //    }
        //   return "Weight In Strategy";
        //}


        public static List<Tuple<int, int>> fListOfBreachedInstFolio = new List<Tuple<int, int>>();
        private void SetBreach(ref SSMCellValue cellValue, SSMCellStyle cellStyle, CSMPosition pos, string message)
        {
            cellValue.SetString(message);
            SSMRgbColor col = new SSMRgbColor();
            col.green = 0;
            col.blue = 0;
            col.red = 65000;
            cellStyle.color = col;
            if (fListOfBreachedInstFolio.Contains(new Tuple<int, int>(pos.GetInstrumentCode(), pos.GetPortfolioCode())) == false)
            {
                fListOfBreachedInstFolio.Add(new Tuple<int, int>(pos.GetInstrumentCode(), pos.GetPortfolioCode()));
            }
        }

        private void RemoveBreach(CSMPosition pos)
        {
            fListOfBreachedInstFolio.Remove(new Tuple<int, int>(pos.GetInstrumentCode(), pos.GetPortfolioCode()));
        }
        
    }
}
