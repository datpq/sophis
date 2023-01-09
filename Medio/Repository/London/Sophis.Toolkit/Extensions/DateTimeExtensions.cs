// -----------------------------------------------------------------------
//  <copyright file="DateTimeExtensions.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Extensions
{
    using System;
    using Tools;

    public static class DateTimeExtensions
    {
        internal static DateTime ToDateTime(this int value)
        {
            CSxDate date = value;
            return date;
        }

        internal static int ToSophisDate(this DateTime value)
        {
            CSxDate date = value;
            
            return date;
        }

        public static DateTime PreviousBusinessDay(this DateTime value)
        {
            do
            {
                value = value.AddDays(-1);
            } while (value.DayOfWeek == DayOfWeek.Sunday || value.DayOfWeek == DayOfWeek.Saturday);
            return value;
        }

        public static DateTime NextBusinessDay(this DateTime value)
        {
            do
            {
                value = value.AddDays(-1);
            } while (value.DayOfWeek == DayOfWeek.Sunday || value.DayOfWeek == DayOfWeek.Saturday);
            return value;
        }
    }
}