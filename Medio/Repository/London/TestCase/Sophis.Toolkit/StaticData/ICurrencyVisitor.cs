// -----------------------------------------------------------------------
//  <copyright file="ICurrencyVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.StaticData
{
    using System;
    using sophis.static_data;

    public interface ICurrencyVisitor
    {
        string Name { get; }
        string Code { get; }
        int Id { get; }
        int Version { get; }
        short DecimalsForRoundingMethod { get; }
        short Quotity { get; }
        double Spot { get; }

        double GetRoundedAmount(double amount);
        double GetHistoryLast(DateTime date);
        double GetForexHistory(DateTime date);
        //DateTime MatchingBusinessDay(DateTime date, eMHolidayAdjustmentType holidayAdjustmentType);
    }
}