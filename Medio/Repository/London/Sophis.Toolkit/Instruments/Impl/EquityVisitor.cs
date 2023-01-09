// -----------------------------------------------------------------------
//  <copyright file="EquityVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments.Impl
{
    using sophis.instrument;

    public class EquityVisitor : InstrumentVisitor<CSMEquity>, IEquityVisitor
    {
        public EquityVisitor(CSMInstrument instrument) : base(instrument)
        {
        }

        protected override CSMEquity GetInstance(CSMInstrument instrument)
        {
            CSMEquity equity = instrument;
            return equity;
        }
    }
}