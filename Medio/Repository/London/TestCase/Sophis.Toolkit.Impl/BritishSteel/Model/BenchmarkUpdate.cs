// -----------------------------------------------------------------------
//  <copyright file="BenchmarkUpdate.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.BritishSteel.Model
{
    public class BenchmarkUpdate
    {
        public BenchmarkUpdate()
        {
        }

        public BenchmarkUpdate(int sicovam, int index, bool isFloat)
        {
            Sicovam = sicovam;
           Index = index;
            IsFloat = isFloat;
        }

        public int Index { get; set; }
        public bool IsFloat { get; set; }
        public int Sicovam { get; set; }
    }
}