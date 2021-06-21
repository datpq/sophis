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

//@DPH
using Eff.UpgradeUtilities;

namespace TkoPortfolioColumn
{
    public static class PortFoliosExtentionMethods
    {
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
                }
            }
            if (sumasset != 0) val = val / sumasset;

            input.IndicatorValue = val;
            return input.IndicatorValue;
        }
        #endregion

        #region TkoGetPondDurationCr
        //Fonction Duration Asset
        public static double TkoGetPondDurationCr(this CSMPortfolio portfolio, InputProvider input)
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
                    if (input.Instrument.GetInstrumentType() == 'F')
                    {
                        asset = position.GetInstrumentCount() * input.Instrument.GetNotional();
                    }
                    else
                    {
                        asset = passet;
                    }

                    if (asset != 0)
                    {
                        double nominal = input.Position.GetInstrumentCount() * input.Instrument.GetNotional();
                        double duration = 0.0;
                        if (nominal != 0)
                        {
                            duration = input.Instrument.TkoComputeDurationValue(input);
                            input.IndicatorValue = duration;
                        }
                        val += asset * duration;
                        sumasset += asset;
                    }
                }
            }
            if (sumasset != 0) val = val / sumasset;

            input.IndicatorValue = val;
            return input.IndicatorValue;
        }
        #endregion

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
    }
}
