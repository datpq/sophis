// -----------------------------------------------------------------------
//  <copyright file="BenchmarkVisitor.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Instruments.Impl
{
    using System;
    using System.Data;
    using Enums;
    using Extensions;
    using sophis.instrument;
    using sophis.value.benchmark;

    public class BenchmarkVisitor : AbtractVisitor<CSAMBenchmarkBridge>, IBenchmarkVisitor
    {
        #region Fields
        
        #endregion

        #region Constructors

        public BenchmarkVisitor(CSMInstrument instrument)
            : base(null)
        {
            SetBenchmark(instrument.GetCode());
        }

        #endregion

        #region Properties

        public CSAMBenchmarkBridge Benchmark
        {
            get { return Host; }
        }

        public int Sicovam
        {
            get { return Benchmark.GetCode(); }
        }

        public string Name
        {
            get { return Benchmark.Name; }
            set { Benchmark.Name = value; }
        }

        public int Currency
        {
            get { return Benchmark.Currency; }
            set { Benchmark.Currency = value; }
        }

        public string Reference
        {
            get { return Benchmark.Reference; }
            set { Benchmark.Reference = value; }
        }

        public DateTime ActiveRecordDate
        {
            get { return Benchmark.GetActiveRecordDate().ToDateTime(); }
        }

        #endregion

        #region Methods

        public DataTable GetComposition(DateTime date)
        {
            return Benchmark.GetComposition(date.ToSophisDate());
        }

        public void Save(InstrumentParameterModificationType type)
        {
            Save();
        }

        public void Save()
        {
            Benchmark.Save();
        }

        public void SetComposition(DataTable table, DateTime date)
        {
            Benchmark.SetComposition(table, date.ToSophisDate());
        }

        private void SetBenchmark(int sicovam)
        {
            SetHost(CSAMBenchmarkBridge.CreateInstance(sicovam, 0));
        }

        #endregion
    }
}