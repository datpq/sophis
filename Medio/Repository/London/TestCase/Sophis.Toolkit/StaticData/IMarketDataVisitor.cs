// -----------------------------------------------------------------------
//  <copyright file="IMarketDataVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.StaticData
{
    public interface IMarketDataVisitor
    {
        double GetForex(int currency1, int curreny2);
    }
}