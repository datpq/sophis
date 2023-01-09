// -----------------------------------------------------------------------
//  <copyright file="IBenchmarkVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments
{
    using System;
    using System.Data;

    public interface IBenchmarkVisitor : IInstrumentVisitor
    {     
        #region Properties

        DateTime ActiveRecordDate { get; }

        #endregion

        #region Methods

        void Save();
        DataTable GetComposition(DateTime activeDate);
        void SetComposition(DataTable bmComp, DateTime date);
    
        #endregion
    }
}