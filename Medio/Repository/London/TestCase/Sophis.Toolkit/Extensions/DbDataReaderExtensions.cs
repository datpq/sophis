// -----------------------------------------------------------------------
//  <copyright file="DbDataReaderExtensions.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Extensions
{
    using System;
    using System.Data.Common;

    public static class DbDataReaderExtensions
    {
        public static T GetActualValue<T>(this DbDataReader reader, string name)
        {
            return reader.GetActualValue(reader.GetOrdinal(name), default(T));
        }

        public static T GetActualValue<T>(this DbDataReader reader, string name, T defaultValue)
        {
            return reader.GetActualValue(reader.GetOrdinal(name), defaultValue);
        }

        public static T GetActualValue<T>(this DbDataReader reader, int ordinal)
        {
            return reader.GetActualValue(ordinal, default(T));
        }

        public static T GetActualValue<T>(this DbDataReader reader, int ordinal, T defaultValue)
        {
            var obj = reader.GetValue(ordinal);
            if (obj == null || obj is DBNull) return defaultValue;
            return (T)Convert.ChangeType(obj, typeof(T));
        }
    }
}