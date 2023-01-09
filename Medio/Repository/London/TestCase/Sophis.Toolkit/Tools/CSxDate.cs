// -----------------------------------------------------------------------
//  <copyright file="CSxDate.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/24</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Tools
{
    using System;
    using sophis.utils;
    using sophisTools;

    internal class CSxDate : CSMDay
    {
        public CSxDate(CMString yyyymmdd) : base(yyyymmdd)
        {
        }

        public CSxDate(int day, int month, int year) : base(day, month, year)
        {
        }

        public CSxDate(int day) : base(day)
        {
        }

        public CSxDate()
        {
        }

        public static implicit operator DateTime(CSxDate date)
        {
            return new DateTime(date.fYear, date.fMonth, date.fDay);
        }

        public static implicit operator int(CSxDate date)
        {
            return date.toLong();
        }

        public static implicit operator long(CSxDate date)
        {
            return date.toLong();
        }

        public static implicit operator CSxDate(int date)
        {
            return new CSxDate(date);
        }

        public static implicit operator CSxDate(DateTime date)
        {
            return new CSxDate(date.Day, date.Month, date.Year);
        }
    }
}