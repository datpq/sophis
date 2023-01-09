// -----------------------------------------------------------------------
//  <copyright file="IDataProvider.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.BritishSteel.Data
{
    using System;
    using System.Collections.Generic;
    using Model;

    public interface IDataProvider
    {
        IList<BenchmarkUpdate> LoadIndexToBenchmarkMappings();
        EquityHistoric LoadEquityHistoric(int sicovam, DateTime date);
    }
}