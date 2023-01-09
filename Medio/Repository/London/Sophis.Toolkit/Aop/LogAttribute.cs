// -----------------------------------------------------------------------
//  <copyright file="LogAttribute.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/02/11</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Aop
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public abstract class SophisAttribute : Attribute
    {
    }

    public class LogAttribute : SophisAttribute
    {
        //public CSxLog.Verbosity Verbosity { get; set; }

        //public LogAttribute()
        //{
        //    Verbosity = CSxLog.Verbosity.Debug;
        //}

        //public LogAttribute(CSxLog.Verbosity verbosity)
        //{
        //    Verbosity = verbosity;
        //}
    }
}