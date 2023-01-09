// -----------------------------------------------------------------------
//  <copyright file="MarketDataVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.StaticData.Impl
{
    using Instruments.Impl;
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
    }
}