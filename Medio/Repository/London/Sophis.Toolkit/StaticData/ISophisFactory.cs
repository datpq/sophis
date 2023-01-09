// -----------------------------------------------------------------------
//  <copyright file="ISophisFactory.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.StaticData
{
    using Instruments;

    public interface ISophisFactory
    {
        bool IsInBatchMode { get; }

        IInstrumentVisitor GetInstrument(int sicovam);
        IInstrumentVisitor GetInstrument(string refType, string reference);
        int GetInstrumentId(string refType, string reference);
        ICurrencyVisitor GetCurrency(int currency);
        ICurrencyVisitor GetCurrency(string currency);
        string CurrencyToString(int currency);
        int StringToCurrency(string currency);
        IMarketDataVisitor GetCurrentMarketData();
    }
}