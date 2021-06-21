using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sophis.portfolio;
using System.Collections;
using sophis.instrument;
using sophis.market_data;
using sophis.utils;
using TkoPortfolioColumn.DataCache;
using sophis.static_data;
using TkoPortfolioColumn.DbRequester;
using System.ComponentModel;
using sophis.value;
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class PortFoliosExtentionMethods
    {
        #region TkoGlobalRiskAmfPortFolioConsolidation


        public static double TkoGlobalRiskAmfPortFolioConsolidationExposure(this CSMPortfolio portfolio, InputProvider input)
        {
            try
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                }
                int level = portfolio.GetLevel();
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "level={0}", level);
                }
                CSMPortfolio port = portfolio;
                double fxspot = 0.0;
                double exposure = 0.0;
                double sum = 0.0;
                do
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "portLevel={0}, Code={1}, Name={2}", port.GetLevel(), port.GetCode(), port.GetName());
                    }
                    for (int positionIndex = 0; positionIndex < port.GetTreeViewPositionCount(); ++positionIndex)
                    {
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "positionIndex={0}, GetTreeViewPositionCount={1}", positionIndex, port.GetTreeViewPositionCount());
                        }
                        input.Position = port.GetNthTreeViewPosition(positionIndex);
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "GetInstrumentCount={0}", input.Position.GetInstrumentCount());
                        }
                        if (input.Position.GetInstrumentCount() != 0)
                        {
                            input.InstrumentCode = input.Position.GetInstrumentCode();
                            input.Instrument = CSMInstrument.GetInstance(input.InstrumentCode);
                            input.PositionIdentifier = input.Position.GetIdentifier();
                            fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
                            if (UpgradeExtensions.IsDebugEnabled())
                            {
                                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "input={0}, fxspot={1}", input.ToString(), fxspot);
                            }
                            exposure = input.Instrument.TkoRouteAmfConsolidation(input);
                            if (UpgradeExtensions.IsDebugEnabled())
                            {
                                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "input={0}, exposure={1}", input.ToString(), exposure);
                            }
                            if (!double.IsNaN(exposure))
                            {
                                //for the forward forex instrument only.
                                if (input.InstrumentType.Equals("Forward Forex"))
                                {
                                    sum += exposure;
                                }
                                else
                                {
                                    sum += exposure * fxspot;
                                }
                            }
                            else
                            {
                                sum = double.NaN;
                                input.IndicatorValue = sum;
                                break;
                            }
                        }
                    }
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "GetNextPortfolio.BEGIN");
                    }
                    port = port.GetNextPortfolio();
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "GetNextPortfolio.END");
                    }
                }
                while (port != null && port.GetLevel() > level);

                input.IndicatorValue = sum;
                return input.IndicatorValue;
            }
            catch (Exception e)
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                input.IndicatorValue = 0;
                return 0;
            }
            finally
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                }
            }
        }

        public static double TkoGlobalRiskAmfPortFolioConsolidationLevrage(this CSMPortfolio portfolio, InputProvider input)
        {
            try
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                }
                int level = portfolio.GetLevel();
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "level={0}", level);
                }
                CSMPortfolio port = portfolio;
                double fxspot = 0.0;
                double exposure = 0.0;
                double sum = 0.0;
                do
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "portLevel={0}, Code={1}, Name={2}", port.GetLevel(), port.GetCode(), port.GetName());
                    }
                    for (int positionIndex = 0; positionIndex < port.GetTreeViewPositionCount(); ++positionIndex)
                    {
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "positionIndex={0}, GetTreeViewPositionCount={1}", positionIndex, port.GetTreeViewPositionCount());
                        }
                        input.Position = port.GetNthTreeViewPosition(positionIndex);
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "GetInstrumentCount={0}", input.Position.GetInstrumentCount());
                        }
                        if (input.Position.GetInstrumentCount() != 0)
                        {
                            input.InstrumentCode = input.Position.GetInstrumentCode();
                            input.Instrument = CSMInstrument.GetInstance(input.InstrumentCode);
                            input.PositionIdentifier = input.Position.GetIdentifier();
                            fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
                            if (UpgradeExtensions.IsDebugEnabled())
                            {
                                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "input={0}, fxspot={1}", input.ToString(), fxspot);
                            }
                            exposure = input.Instrument.TkoRouteAmfConsolidation(input);
                            if (UpgradeExtensions.IsDebugEnabled())
                            {
                                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "input={0}, exposure={1}", input.ToString(), exposure);
                            }
                            if (!double.IsNaN(exposure))
                            {
                                sum += exposure * fxspot;
                            }
                            else
                            {
                                sum = double.NaN;
                                input.IndicatorValue = sum;
                                break;
                            }
                        }
                    }
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "GetNextPortfolio.BEGIN");
                    }
                    port = port.GetNextPortfolio();
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "GetNextPortfolio.END");
                    }
                }
                while (port != null && port.GetLevel() > level);

                input.IndicatorValue = sum;
                return input.IndicatorValue;
            }
            catch (Exception e)
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                input.IndicatorValue = 0;
                return 0;
            }
            finally
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                }
            }
        }

        public static double TkoGlobalRiskAmfPortFolioConsolidation(this CSMPortfolio portfolio, InputProvider input)
        {
            try
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "BEGIN(input={0})", input.ToString());
                }
                int level = portfolio.GetLevel();
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "level={0}", level);
                }
                CSMPortfolio port = portfolio;
                double fxspot = 0.0;
                double exposure = 0.0;
                double sum = 0.0;
                do
                {
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "portLevel={0}, Code={1}, Name={2}", port.GetLevel(), port.GetCode(), port.GetName());
                    }
                    for (int positionIndex = 0; positionIndex < port.GetTreeViewPositionCount(); ++positionIndex)
                    {
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "positionIndex={0}, GetTreeViewPositionCount={1}", positionIndex, port.GetTreeViewPositionCount());
                        }
                        input.Position = port.GetNthTreeViewPosition(positionIndex);
                        if (UpgradeExtensions.IsDebugEnabled())
                        {
                            UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "GetInstrumentCount={0}", input.Position.GetInstrumentCount());
                        }
                        if (input.Position.GetInstrumentCount() != 0)
                        {
                            input.InstrumentCode = input.Position.GetInstrumentCode();
                            input.Instrument = CSMInstrument.GetInstance(input.InstrumentCode);
                            input.PositionIdentifier = input.Position.GetIdentifier();
                            fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
                            if (UpgradeExtensions.IsDebugEnabled())
                            {
                                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "input={0}, fxspot={1}", input.ToString(), fxspot);
                            }
                            exposure = input.Instrument.TkoRouteAmfConsolidation(input);
                            if (UpgradeExtensions.IsDebugEnabled())
                            {
                                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "input={0}, exposure={1}", input.ToString(), exposure);
                            }
                            if (!double.IsNaN(exposure))
                            {
                                switch (input.Column)
                                {
                                    case "TKO Exposure":
                                        //for the forward forex instrument only.
                                        if (input.InstrumentType.Equals("Forward Forex"))
                                        {
                                            sum += exposure;
                                        }
                                        else
                                        {
                                            sum += exposure * fxspot;
                                        }
                                        break;
                                    default:
                                        sum += exposure * fxspot;
                                        break;
                                }
                            }
                            else
                            {
                                sum = double.NaN;
                                input.IndicatorValue = sum;
                                break;
                            }
                        }
                    }
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "GetNextPortfolio.BEGIN");
                    }
                    port = port.GetNextPortfolio();
                    if (UpgradeExtensions.IsDebugEnabled())
                    {
                        UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "GetNextPortfolio.END");
                    }
                }
                while (port != null && port.GetLevel() > level);

                input.IndicatorValue = sum;
                return input.IndicatorValue;
            }
            catch (Exception e)
            {
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_error, e.ToString());
                input.IndicatorValue = 0;
                return 0;
            }
            finally
            {
                if (UpgradeExtensions.IsDebugEnabled())
                {
                    UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "END");
                }
            }

            //if (instrType.Contains("Funds"))
            //{

            //double balance = portfolio.GetBalance() * 1000;
            //double unsettledBalance = portfolio.GetUnsettledBalance() * 1000;
            //double assetValueFolio = portfolio.GetAssetValue() * 1000;

            ////Consolidation Sophis.
            //double portFolioConsolidation = 0;
            //var cellValueConsolidate = new SSMCellValue();

            //input.delegateFolioConsolidation(input.ActivePortfolioCode, input.PortFolioCode, input.Extraction, ref cellValueConsolidate, input.CellType, true);

            //portFolioConsolidation = Math.Abs(cellValueConsolidate.doubleValue);

           

            //input.IndicatorValue = sum; 
            //var globalRiskAmf = portFolioConsolidation + balance + unsettledBalance ;
            //var globalRiskAmf = portFolioConsolidation;
            //var globalRiskAmf = sum;
            //input.IndicatorValue = globalRiskAmf;

            //}
            //else
            //{
            //    //Sophis Consolidation => Folio currency.
            //    var cellValueConsolidate = new SSMCellValue();
            //    input.delegateFolioConsolidation(input.ActivePortfolioCode, input.PortFolioCode, input.Extraction, ref cellValueConsolidate, input.CellType, true);
            //    input.IndicatorValue = cellValueConsolidate.doubleValue;
            //} 
        }

        public static double TkoGlobalRiskAmfPortFolioConsolidationWithoutBalance(this CSMPortfolio portfolio, InputProvider input, SSMCellStyle cellStyle)
        {
            input.TmpPortfolioColName = "Instrument type";
            string instrType = Helper.TkoGetStringValueFromSophis(input);

            if (instrType.Contains("Funds"))
            {

                double balance = portfolio.GetBalance() * 1000;
                double unsettledBalance = portfolio.GetUnsettledBalance() * 1000;
                double assetValueFolio = portfolio.GetAssetValue() * 1000;

                //Consolidation Sophis.
                double portFolioConsolidation = 0;
                var cellValueConsolidate = new SSMCellValue();

                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "delegateFolioConsolidation.BEGIN(input={0})", input.ToString());
                input.delegateFolioConsolidation(input.ActivePortfolioCode, input.PortFolioCode, input.Extraction, ref cellValueConsolidate, cellStyle, true);
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "delegateFolioConsolidation.END(input={0})", input.ToString());

                portFolioConsolidation = cellValueConsolidate.doubleValue;

                var folioassetvalue = portfolio.GetNetAssetValue();

                var globalRiskAmf = portFolioConsolidation * 100 / portfolio.GetNetAssetValue();
                /*+ balance + unsettledBalance + assetValueFolio*/
                input.IndicatorValue = globalRiskAmf;

            }
            else
            {
                //Sophis Consolidation => Folio currency.
                var cellValueConsolidate = new SSMCellValue();
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "delegateFolioConsolidation.BEGIN(input={0})", input.ToString());
                input.delegateFolioConsolidation(input.ActivePortfolioCode, input.PortFolioCode, input.Extraction, ref cellValueConsolidate, cellStyle, true);
                UpgradeExtensions.Log(CSMLog.eMVerbosity.M_debug, "delegateFolioConsolidation.END(input={0})", input.ToString());
                input.IndicatorValue = cellValueConsolidate.doubleValue;
            }
            return input.IndicatorValue;
        }

        #endregion

        #region Compute Delta Folio

        public static double TkoGlobaDeltaConsolidate(this CSMPortfolio portfolio, InputProvider input)
        {
            SSMCellValue dummyCellValue = new SSMCellValue();
            SSMCellStyle dummyCellStyle = new SSMCellStyle();

            CSMPortfolioColumn colDelta = CSMPortfolioColumn.GetCSRPortfolioColumn("Global Delta");
            double sum = 0.0;
            double fxspot = 0.0;
            var underlyingNumber = portfolio.GetUnderlyingCount();
            
            CSMAmFund fund = CSMAmFund.GetFundFromFolio(portfolio);
            if (fund != null && !portfolio.IsAPortfolio()) input.PortFolioCode = fund.GetTradingPortfolio();
            CSMPortfolio activePortfolio = CSMPortfolio.GetRootPortfolio();
            if (activePortfolio != null) { input.ActivePortfolioCode = activePortfolio.GetCode(); }

            for (int index = 0; index < underlyingNumber; ++index)
            {
                CSMUnderlying underlying = portfolio.GetNthUnderlying(index);
                input.UnderlyingCode = underlying.GetInstrumentCode();
                if (underlying != null && input.UnderlyingCode != 0)
                {
                    colDelta.GetUnderlyingCell(
                        input.ActivePortfolioCode,
                        input.PortFolioCode,
                        null,
                        input.UnderlyingCode,
                        ref dummyCellValue,
                        dummyCellStyle,
                        true);


                    fxspot = CSMMarketData.GetCurrentMarketData().GetForex(dummyCellStyle.currency);
                    //var res = DbrPortFolio.RetrieveFolioCodeFromFolioName(input.PortFolioName);
                    input.Instrument = CSMInstrument.GetInstance(input.UnderlyingCode);
                    if (input.Instrument == null && dummyCellValue.doubleValue != 0.0 && fxspot != 1.0)
                    {
                        if (!double.IsNaN(dummyCellValue.doubleValue)) sum += dummyCellValue.doubleValue * fxspot;
                    }
                }
            }

            input.IndicatorValue = sum;
            return input.IndicatorValue;
        }
        #endregion 
    }
}
