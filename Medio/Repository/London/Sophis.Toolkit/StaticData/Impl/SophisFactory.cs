// -----------------------------------------------------------------------
//  <copyright file="SophisFactory.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.StaticData.Impl
{
    using System;
    using System.Globalization;
    using Instruments;
    using StaticData;
    using sophis.instrument;
    using sophis.market_data;
    using sophis.static_data;
    using sophis.utils;

    public sealed class SophisFactory : ISophisFactory
    {
        public bool IsInBatchMode
        {
            get { return CSMApi.IsInBatchMode(); }
        }

        public IInstrumentVisitor GetInstrument(int sicovam)
        {
            var instrument = CSMInstrument.GetInstance(sicovam);

            return instrument == null ? null : ResolveInstrument(instrument);
        }

        public IInstrumentVisitor GetInstrument(string refType, string reference)
        {
            var sicovam = GetInstrumentId(refType, reference);
            return GetInstrument(sicovam);
        }

        public int GetInstrumentId(string refType, string reference)
        {
            var refList = new CSMUniversalReferenceList();
            return refList.GetInstrumentId(refType, reference, true);
        }

        public ICurrencyVisitor GetCurrency(int currency)
        {
            var c = CSMCurrency.GetCSRCurrency(currency);
            return new CurrencyVisitor(c);
        }

        public ICurrencyVisitor GetCurrency(string currency)
        {
            var id = CSMCurrency.StringToCurrency(currency);
            var c = CSMCurrency.GetCSRCurrency(id);
            return new CurrencyVisitor(c);
        }

        public string CurrencyToString(int currency)
        {
            var str = new CMString();
            CSMCurrency.CurrencyToString(currency, str);
            return str;
        }

        public int StringToCurrency(string currency)
        {
            return CSMCurrency.StringToCurrency(currency);
        }

        public IMarketDataVisitor GetCurrentMarketData()
        {
            return new MarketDataVisitor(CSMMarketData.GetCurrentMarketData());
        }

        private IInstrumentVisitor ResolveInstrument(CSMInstrument instr)
        {
            var instType = instr.GetType_API().ToString(CultureInfo.InvariantCulture);

            var i = SophisContainer.Resolve<IInstrumentVisitor>(instType, instr);

            if (i == null)
                throw new ArgumentOutOfRangeException(string.Format("No visitor found for type : {0}", instType));

            return i;
        }
    }
}