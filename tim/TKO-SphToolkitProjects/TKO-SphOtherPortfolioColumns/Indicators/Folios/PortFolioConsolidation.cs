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

//@DPH
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class PortFoliosExtentionMethods
    {
        #region TkocPondYTM
        //Fonction YTM Asset
        public static double TkocGetPondYTM(this CSMPortfolio portfolio, InputProvider input)
        {

            input.PortFolioName = portfolio.GetName().StringValue;

            double val = 0;
            input.IndicatorValue = val;
            CSMPosition position;
            //On boucle sur les folios 
            double asset = 0;
            double fxspot = 0;
            double sumasset = 0;
            double ytm = 0;

            int positionNumber = portfolio.GetTreeViewPositionCount();
            for (int index = 0; index < positionNumber; index++)
            {
                position = portfolio.GetNthTreeViewPosition(index);
                if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                {
                    input.InstrumentCode = position.GetInstrumentCode();
                    input.Instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());

                    input.Position = position;
                    input.PositionIdentifier = position.GetIdentifier();
                    fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    asset = position.GetAssetValue() * fxspot * 1000;

                    input.InstrumentReference = input.Instrument.GetReference().StringValue;

                    ytm = input.Instrument.TkoComputeTreeYTMValue(input);
                    input.IndicatorValue = ytm;

                    if (asset != 0)
                    {
                        //if (ytm < 0) ytm = 0; //CR 22052019 Floor 0 TKO YTM
                        val += asset * ytm;
                        sumasset += asset;
                    }

                    //input.AllOtherFieldInfos.Remove("AssetValue");
                    //input.AllOtherFieldInfos.Remove("Fxspot");
                    //input.AllOtherFieldInfos.Remove("Ytm");
                    //input.AllOtherFieldInfos.Add("AssetValue", position.GetAssetValue().ToString());
                    //input.AllOtherFieldInfos.Add("Fxspot", fxspot.ToString());
                    //input.AllOtherFieldInfos.Add("Ytm", ytm.ToString());
                }
            }
            if (sumasset != 0) val = val / sumasset;

            input.IndicatorValue = val;
            return input.IndicatorValue;
        }
        #endregion

        //#region TkoGetPondDurationCr
        ////Fonction Duration Asset
        //public static double TkoGetPondDurationCr(this CSMPortfolio portfolio, InputProvider input)
        //{
        //    double val = 0;
        //    input.IndicatorValue = 0;
        //    CSMPosition position;
        //    //On boucle sur les folios 
        //    double asset = 0;
        //    double passet = 0;
        //    double fxspot = 0;
        //    double sumasset = 0;


        //    int positionNumber = portfolio.GetTreeViewPositionCount();
        //    for (int index = 0; index < positionNumber; index++)
        //    {
        //        position = portfolio.GetNthTreeViewPosition(index);
        //        if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
        //        {
        //            input.Instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
        //            input.Position = position;
        //            input.PositionIdentifier = position.GetIdentifier();
        //            fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
        //            passet = position.GetAssetValue() * fxspot * 1000;

        //            //Différence des cas 

        //            //Cas Futures
        //            if (input.Instrument.GetInstrumentType() == 'F')
        //            {
        //                asset = position.GetInstrumentCount() * input.Instrument.GetNotional();
        //            }
        //            else
        //            {
        //                asset = passet;
        //            }

        //            if (asset != 0)
        //            {
        //                double nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
        //                double duration = 0.0;
        //                if (nominal != 0)
        //                {
        //                    duration = input.Instrument.TkoComputeDurationValue(input);
        //                    input.IndicatorValue = duration;
        //                }
        //                val += asset * duration;
        //                sumasset += asset;
        //            }
        //        }
        //    }
        //    if (sumasset != 0) val = val / sumasset;

        //    input.IndicatorValue = val;
        //    return input.IndicatorValue;
        //}
        //#endregion

        #region TkoGetPondDurationIr
        //Fonction DurationIr Asset
        public static double TkoGetPondDurationIr(this CSMPortfolio portfolio, InputProvider input)
        {
            double val = 0;
            input.IndicatorValue = 0;
            CSMPosition position;
            //On boucle sur les folios 
            double asset = 0;
            double passet = 0;
            double fxspot = 0;
            double sumasset = 0;


            int positionNumber = portfolio.GetTreeViewPositionCount();
            for (int index = 0; index < positionNumber; index++)
            {
                position = portfolio.GetNthTreeViewPosition(index);
                if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                {
                    input.Instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                    input.Position = position;
                    input.PositionIdentifier = position.GetIdentifier();

                    fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    passet = position.GetAssetValue() * fxspot * 1000;
                    //Cas Futures
                    if (input.Instrument.GetType_API() == 'F')
                    {
                        sophis.finance.CSMNotionalFuture fut = input.Instrument;
                        if (fut != null)
                        {
                            //Not Sure !!!
                            var sicovam = fut.GetCheapest();
                            SSMCalcul concordanceFactor = new SSMCalcul();
                            fut.GetConcordanceFactor(CSMMarketData.GetCurrentMarketData(), sicovam, concordanceFactor);
                            if (concordanceFactor.fConcordanceFactor != 0)
                            {
                                asset = concordanceFactor.fNotionalFutureValue * fut.GetInstrumentCount();
                            }
                        }
                        else
                        {
                            asset = position.GetInstrumentCount() * input.Instrument.GetNotional();
                        }
                    }
                    else
                    {
                        asset = passet;
                    }

                    if (asset != 0)
                    {
                        if (input.Instrument.GetType_API() == 'O' || input.Instrument.GetInstrumentType() == 'D' || input.Instrument.GetInstrumentType() == 'F')
                        {
                            double nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                            double duration = 0.0;
                            if (nominal != 0)
                            {
                                duration = input.Instrument.TkoComputeTKODurationIr(input);
                                input.IndicatorValue = duration;
                            }
                            val += input.Instrument.TkoComputeGearing(input, portfolio) * duration;
                        }
                        sumasset += asset;
                    }
                }
            }
            if (sumasset != 0) val = val / sumasset;

            input.IndicatorValue = val;
            return val;
        }
        #endregion

        #region TkoGetPondGearing
        //Fonction DurationIr Asset
        public static double TkoGetPondGearingFixOption(this CSMPortfolio portfolio, InputProvider input)
        {
            Dictionary<int, List<CSMPosition>> DicOfUnderlying = new Dictionary<int, List<CSMPosition>>();
            int positionNumber = portfolio.GetTreeViewPositionCount();
            CSMPosition position;
            CSMInstrument instrument;
            double sumgearing = 0.0;
            for (int index = 0; index < positionNumber; index++)
            {
                position = portfolio.GetNthTreeViewPosition(index);
                if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                {
                    instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                    input.InstrumentCode = position.GetInstrumentCode();

                    input.PositionIdentifier = position.GetIdentifier();
                    input.Position = position;

                    if (instrument.GetType_API() == 'D')
                    {
                        int nbUnderlyings = instrument.GetUnderlyingCount();
                        for (int i = 0; i < nbUnderlyings; i++)
                        {
                            var sousjacents = CSMInstrument.GetInstance(instrument.GetUnderlying(i));
                            if (sousjacents != null)
                            {
                                var typeapi = sousjacents.GetType_API();
                                if (typeapi == 'I' || typeapi == 'A')
                                {
                                    if (DicOfUnderlying.ContainsKey(instrument.GetUnderlying(i)))
                                    {
                                        DicOfUnderlying[instrument.GetUnderlying(i)].Add(position);
                                    }
                                    else
                                    {
                                        DicOfUnderlying.Add(instrument.GetUnderlying(i), new List<CSMPosition>() { position });
                                    }
                                }
                                else
                                {
                                    sumgearing += instrument.TkoComputeGearing(input, portfolio);
                                }
                            }
                            else
                            {
                                sumgearing += instrument.TkoComputeGearing(input, portfolio);
                            }
                        }
                    }
                    else
                    {
                        sumgearing += instrument.TkoComputeGearing(input, portfolio);
                    }
                }
            }

            //double sum = 0.0;
            foreach (int underlyingCode in DicOfUnderlying.Keys)
            {
                double sumByUnderlying = 0.0;
                foreach (var positionOption in DicOfUnderlying[underlyingCode])
                {
                    instrument = CSMInstrument.GetInstance(positionOption.GetInstrumentCode());
                    input.InstrumentCode = positionOption.GetInstrumentCode();

                    input.PositionIdentifier = positionOption.GetIdentifier();
                    input.Position = positionOption;

                    sumByUnderlying += instrument.TkoComputeGearingWithoutAbs(input, portfolio);
                }
                sumgearing += Math.Abs(sumByUnderlying);
            }


            input.IndicatorValue = sumgearing;
            return input.IndicatorValue;
        }

        public static double TkoGetPondGearing(this CSMPortfolio portfolio, InputProvider input)
        {
            int positionNumber = portfolio.GetTreeViewPositionCount();
            CSMPosition position;
            CSMInstrument instrument;
            double sum = 0.0;
            for (int index = 0; index < positionNumber; index++)
            {
                position = portfolio.GetNthTreeViewPosition(index);
                if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                {
                    instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                    input.InstrumentCode = position.GetInstrumentCode();

                    input.PositionIdentifier = position.GetIdentifier();
                    input.Position = position;
                    input.IndicatorValue += instrument.TkoComputeGearing(input, portfolio);
                }
            }
            return input.IndicatorValue;
        }
        #endregion

        #region TkoGlobalRiskAmfPortFolioConsolidation

        public static double TkoGlobalRiskAmfPortFolioConsolidation(this CSMPortfolio portfolio, InputProvider input)
        {
            int level = portfolio.GetLevel();
            CSMPortfolio port = portfolio;
            double fxspot = 0.0;
            double exposure = 0.0;
            double sum = 0.0;
            do
            {
                for (int positionIndex = 0; positionIndex < port.GetTreeViewPositionCount(); ++positionIndex)
                {
                    input.Position = port.GetNthTreeViewPosition(positionIndex);
                    if (input.Position.GetInstrumentCount() != 0)
                    {
                        input.InstrumentCode = input.Position.GetInstrumentCode();
                        input.Instrument = CSMInstrument.GetInstance(input.InstrumentCode);
                        input.PositionIdentifier = input.Position.GetIdentifier();
                        fxspot = CSMMarketData.GetCurrentMarketData().GetForex(input.Position.GetCurrency());
                        exposure = input.Instrument.TkoRouteAmfConsolidation(input);
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
                port = port.GetNextPortfolio();
            }
            while (port != null && port.GetLevel() > level);

            input.IndicatorValue = sum;
            return input.IndicatorValue;

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

        #region TkoTop3Positions

        public static double TkoTop3Positions(this CSMPortfolio portfolio, InputProvider input)
        {
            List<double> ListOfPositions = new List<double>();
            int positionNumber = portfolio.GetTreeViewPositionCount();
            CSMPosition position;
            CSMInstrument instrument;

            double AV1 = 0;
            double AV2 = 0;
            double AV3 = 0;
            double AVtemp = 0;

            double value = double.NaN;

            for (int index = 0; index < positionNumber; index++)
            {
                position = portfolio.GetNthTreeViewPosition(index);
                if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                {
                    instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                    input.InstrumentCode = position.GetInstrumentCode();
                    input.PositionIdentifier = position.GetIdentifier();
                    input.Position = position;
                    double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());
                    AVtemp = position.GetAssetValue() * fxspot;
                    if (AVtemp >= AV1)
                    {
                        AV3 = AV2;
                        AV2 = AV1;
                        AV1 = AVtemp;
                    }
                    else
                    {
                        if (AVtemp >= AV2)
                        {
                            AV3 = AV2;
                            AV2 = AVtemp;
                        }
                        else
                        {
                            if (AVtemp >= AV3)
                            {
                                AV3 = AVtemp;
                            }
                        }
                    }
                }
            }
            value = (AV1 + AV2 + AV3) * 1000;
            input.IndicatorValue = value;
            return value;
        }

        #endregion

        #region  TkoTop5Positions
        public static double TkoTop5Positions(this CSMPortfolio portfolio, InputProvider input)
        {
            List<double> ListOfPositions = new List<double>();
            int positionNumber = portfolio.GetTreeViewPositionCount();
            CSMPosition position;
            CSMInstrument instrument;

            double AV1 = 0;
            double AV2 = 0;
            double AV3 = 0;
            double AV4 = 0;
            double AV5 = 0;
            double AVtemp = 0;

            double value = double.NaN;

            for (int index = 0; index < positionNumber; index++)
            {
                position = portfolio.GetNthTreeViewPosition(index);
                if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                {
                    instrument = CSMInstrument.GetInstance(position.GetInstrumentCode());
                    input.InstrumentCode = position.GetInstrumentCode();
                    input.PositionIdentifier = position.GetIdentifier();
                    input.Position = position;
                    double fxspot = CSMMarketData.GetCurrentMarketData().GetForex(position.GetCurrency());

                    if (position.GetInstrumentCount() != 0)//On ne tient compte que des positions ouvertes
                    {
                        AVtemp = position.GetAssetValue() * fxspot;
                        if (AVtemp >= AV1)
                        {
                            AV5 = AV4;
                            AV4 = AV3;
                            AV3 = AV2;
                            AV2 = AV1;
                            AV1 = AVtemp;
                        }
                        else
                        {
                            if (AVtemp >= AV2)
                            {
                                AV5 = AV4;
                                AV4 = AV3;
                                AV3 = AV2;
                                AV2 = AVtemp;
                            }
                            else
                            {
                                if (AVtemp >= AV3)
                                {
                                    AV5 = AV4;
                                    AV4 = AV3;
                                    AV3 = AVtemp;
                                }
                                else
                                {
                                    if (AVtemp >= AV4)
                                    {
                                        AV5 = AV4;
                                        AV4 = AVtemp;
                                    }
                                    else
                                    {
                                        if (AVtemp >= AV5)
                                        {
                                            AV5 = AVtemp;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            value = (AV1 + AV2 + AV3 + AV4 + AV5) * 1000;
            input.IndicatorValue = value;
            return value;
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
            
            //CSMAmFund fund = CSMAmFund.GetFundFromFolio(input.PortFolio);
            CSMAmFund fund = CSMAmFund.GetFundFromFolio(portfolio);
            if (fund != null && !portfolio.IsAPortfolio()) input.PortFolioCode = fund.GetTradingPortfolio();
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

        #region Perf Attribution Colonne

        public static double TkoPerfAttribFlagFolio(this CSMPortfolio portfolio, InputProvider input)
        {
            //CSMAmFund fund = CSMAmFund.GetFundFromFolio(input.PortFolio);
            //if (fund != null && !portfolio.IsAPortfolio())
            //{
            //    input.PortFolioCode = fund.GetTradingPortfolio();
            //    portfolio = CSMPortfolio.GetCSRPortfolio(input.PortFolioCode);
            //    input.PortFolio = portfolio;
            //}

            input.StringIndicatorValue = " ";
            input.TmpPortfolioColName = input.Column;
            input.Column = "TKO STRATEGY";
            DbrPerfAttribMapping.SetColumnConfig(input);

            input.Column = input.TmpPortfolioColName;
            var listOfConfig = input.PerfAttribMappingConfigDic;
            DbRequester.DbrPerfAttribMapping.TIKEHAU_PERFATTRIB_MAPPING value = null;
            if (listOfConfig.TryGetValue(input.PortFolioCode, out value))
            {
                input.StringIndicatorValue = " " + value.PORTFOLIOMAPPINGNAME + " ";
                value = null;
            }
            else
            {
                var parentCode = 0;
                var cpt = listOfConfig.Count;
                int j = 0;
                while (value == null && j < cpt)
                {
                    if (portfolio != null)
                    {
                        parentCode = portfolio.GetParentCode();
                        portfolio = CSMPortfolio.GetCSRPortfolio(parentCode);
                        if (listOfConfig.TryGetValue(parentCode, out value))
                        {
                            input.StringIndicatorValue = " " + value.PORTFOLIOMAPPINGNAME + " ";
                        }
                        j++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return 0;
        }

        #endregion

        #region  TKR Ann Return Since Incep
        public static double TKRAnnReturnSinceIncep(this CSMPortfolio portfolio, InputProvider input)
        {
            // Set Value
            double minNAV;
            double lastNAV;
            double minDate;
            double LastDate;
            double folioSicovam;

            folioSicovam = DbrStringQuery.getResultI("select SICOVAM from titres where type='Z'and REFERENCE = '" + input.PortFolioName + "'");
            var doubleValue = 0.0;
            if (folioSicovam > 0)
            {
                minNAV =   DbrStringQuery.getResultD("select NAV from FUND_NAVFOREOD where SICOVAM = '" + folioSicovam + "'and NAV_DATE in (select Min(NAV_DATE) from FUND_NAVFOREOD where sicovam='" + folioSicovam + "')");
                minDate =  DbrStringQuery.getResultD("select date_to_num(Min(NAV_DATE)) from FUND_NAVFOREOD where SICOVAM ='" + folioSicovam + "'");
                lastNAV =  DbrStringQuery.getResultD("select NAV from FUND_NAVFOREOD where SICOVAM = '" + folioSicovam + "' and NAV_DATE in (select Max(NAV_DATE) from FUND_NAVFOREOD where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "'))");
                LastDate = DbrStringQuery.getResultD("select date_to_num(Max(NAV_DATE)) from FUND_NAVFOREOD where SICOVAM='" + folioSicovam + "' and NAV_DATE<=num_to_date('" + CSMMarketData.GetCurrentMarketData().GetDate() + "')");
                doubleValue = lastNAV / minNAV - 1;//Performance non annualisée
                doubleValue = doubleValue * 365 / (LastDate - minDate);
            }
            doubleValue = doubleValue * 100;

            input.IndicatorValue = doubleValue;
            return input.IndicatorValue;
        }
        #endregion

        #region  TkoAvgPriceBase10Folio
        public static double TkoAvgPriceBase10Folio(this CSMPortfolio portfolio, InputProvider input)
        {
            input.TmpPortfolioColName = "Average price";
            var avgprice = Helper.TkoGetValuefromSophisDouble(input);
            return input.IndicatorValue;
        }
        #endregion
    }
}
