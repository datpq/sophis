// -----------------------------------------------------------------------
//  <copyright file="IBondVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments
{
    using sophis.instrument;

    public interface IBondVisitor : IInstrumentVisitor
    {
        //eMSpreadType SpreadType { get; set; }
        double Spread { get; set; }
        bool UseMetaModel { get; }

        void RecalculateRedemptions();
    }
}