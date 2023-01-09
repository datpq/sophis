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

    internal static class DateTimeExtensions
    {
        public static DateTime ToDateTime(this int value)
        {
            CSxDate date = value;
            return date;
        }

        public static int ToSophisDate(this DateTime value)
        {
            CSxDate date = value;
            return date;
        }
    }
}