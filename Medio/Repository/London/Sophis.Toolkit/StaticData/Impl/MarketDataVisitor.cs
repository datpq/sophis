// -----------------------------------------------------------------------
//  <copyright file="MarketDataVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.StaticData.Impl
{
    using System;
    using Extensions;
    using Instruments.Impl;
    using sophis.scenario;
    using sophis.static_data;
    using StaticData;
    using sophis.market_data;

    public class MarketDataVisitor : AbtractVisitor<CSMMarketData>, IMarketDataVisitor
    {
        public MarketDataVisitor(CSMMarketData marketData) : base(marketData)
        {
        }

        protected CSMMarketData MarketData { get { return Host; } }

        public double GetForex(int currency1, int currency2)
        {
            return MarketData.GetForex(currency1, currency2);

        }

		public double GetForex(DateTime date, int currency1, int currency2)
		{
			var state = new SSMHistorisedState();
			state.SetBit(eMHistorisedParameterType.M_hpSpot);
			state.SetBit(eMHistorisedParameterType.M_hpForex);
			using (var chmd = new CSMHistorisedMarketData(state, date.ToSophisDate(), false))
			{
				chmd.SetState(state);
				double ccy1Fx = chmd.GetForex(currency1);
				double ccy2Fx = chmd.GetForex(currency2);
				double fxRate = ccy1Fx / ccy2Fx;

				return Math.Abs(fxRate) < double.Epsilon ? GetForex(currency1, currency2) : fxRate;
			}
		}

		public double GetForex(CSMHistorisedMarketData hmd, int currency1, int currency2)
		{
			double ccy1Fx = hmd.GetForex(currency1);
			double ccy2Fx = hmd.GetForex(currency2);
			double fxRate = ccy1Fx / ccy2Fx;

			return Math.Abs(fxRate) < double.Epsilon ? GetForex(currency1, currency2) : fxRate;
		}

		public CSMHistorisedMarketData GetHistorisedMarketData(DateTime date)
		{
			var state = new SSMHistorisedState();
			state.SetBit(eMHistorisedParameterType.M_hpSpot);
			state.SetBit(eMHistorisedParameterType.M_hpForex);
			var hmd = new CSMHistorisedMarketData(state, date.ToSophisDate(), false);
			hmd.SetState(state);
			
			return hmd;
		}
    }
}