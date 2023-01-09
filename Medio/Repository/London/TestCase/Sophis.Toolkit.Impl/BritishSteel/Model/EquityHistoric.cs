// -----------------------------------------------------------------------
//  <copyright file="EquityHistoric.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.BritishSteel.Model
{
    using System;

    public struct EquityHistoric
    {
        public DateTime Date { get; set; }
        public decimal Ftnosh { get; set; }
        public decimal Ftsmcf { get; set; }
        public decimal Ftsnosh { get; set; }
        public decimal Last { get; set; }
        public int Sicovam { get; set; }
    }
}