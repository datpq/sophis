// -----------------------------------------------------------------------
//  <copyright file="IMarketDataVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.StaticData
{
    using System;

    public interface IMarketDataVisitor
    {
        double GetForex(int currency1, int currency2);
        double GetForex(DateTime date, int currency1, int currency2);
		double GetForex(sophis.scenario.CSMHistorisedMarketData hmd, int currency1, int currency2);
		sophis.scenario.CSMHistorisedMarketData GetHistorisedMarketData(DateTime date);
    }
}