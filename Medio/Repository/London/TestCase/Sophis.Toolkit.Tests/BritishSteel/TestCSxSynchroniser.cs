// -----------------------------------------------------------------------
//  <copyright file="TestCSxSynchroniser.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Tests.BritishSteel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using Instruments;
    using NUnit.Framework;
    using Rhino.Mocks;
    using StaticData;
    using Sophis.Toolkit.BritishSteel;
    using Sophis.Toolkit.BritishSteel.Data;
    using Sophis.Toolkit.BritishSteel.Model;

    [TestFixture]
    public class TestCSxSynchroniser
    {
        DateTime _date;
        private ISophisFactory _factoryMock;

        [TearDown]
        public void TestCleanUp()
        {
        }

        [SetUp]
        public void TestSetup()
        {
            var marketData = MockRepository.GenerateMock<IMarketDataVisitor>();
            _factoryMock = MockRepository.GenerateMock<ISophisFactory>();

            _date = DateTime.Today;

            _factoryMock.Stub(x => x.GetCurrentMarketData()).Do((Func<IMarketDataVisitor>)(() => marketData));
            _factoryMock.Stub(x => x.StringToCurrency("GBP")).Return(123456);
        }

        [Test(Description = "Test if the benchmark components match the same number as index")]
        public void TestBenchmarkComponentNumber()
        {
            #region Set up for the test scenario (Mocks and Stubs)

            var dataProvider = MockRepository.GenerateMock<IDataProvider>();
            var benchmarkMock = MockRepository.GenerateMock<IBenchmarkVisitor>();
            var indexMock = MockRepository.GenerateMock<IBasketVisitor>();

            benchmarkMock.Stub(x => x.GetComposition(Arg<DateTime>.Is.Anything)).Return(CreateComposition());
            benchmarkMock.Stub(x => x.Currency).Return(_factoryMock.StringToCurrency("GBP"));

            var l = new List<IEquityVisitor>();
            for (var i = 0; i < 3; i++)
            {

                var equityMock = MockRepository.GenerateMock<IEquityVisitor>();
                equityMock.Stub(x => x.Currency).Return(_factoryMock.StringToCurrency("GBP"));
                equityMock.Stub(x => x.Sicovam).Return(103 + i);
                l.Add(equityMock);
                var sicovam = 103 + i;
                _factoryMock.Stub(x => x.GetInstrument(sicovam)).Do((Func<int, IInstrumentVisitor>)(x => equityMock));
            }

            indexMock.Stub(x => x.Components).Return(l);

            _factoryMock.Stub(x => x.GetInstrument(101)).Do((Func<int, IInstrumentVisitor>)(x => benchmarkMock));
            _factoryMock.Stub(x => x.GetInstrument(102)).Do((Func<int, IInstrumentVisitor>)(x => indexMock));

            benchmarkMock.Stub(x => x.SetComposition(Arg<DataTable>.Is.Anything, Arg<DateTime>.Is.Anything))
                         .Do((Action<DataTable, DateTime>)((x, d) => Assert.AreEqual(3, x.Rows.Count)));

            dataProvider.Stub(x => x.LoadEquityHistoric(Arg<int>.Is.Anything, Arg<DateTime>.Is.Anything)).Do((Func<int, DateTime, EquityHistoric>) ((x, d) => new EquityHistoric
                {
                    Sicovam = x,
                    Date = d,
                    Ftsnosh = 1,
                    Ftsmcf = 1,
                    Ftnosh = 1,
                    Last = 1
                }));

            dataProvider.Stub(x => x.LoadIndexToBenchmarkMappings()).Return(new List<BenchmarkUpdate>
                {
                    new BenchmarkUpdate
                        {
                            Sicovam = 101,
                            Index = 102,
                            IsFloat = false
                        }
                });

            #endregion

            #region The block of code that we want to test

            var synchronizer = new CSxSynchroniser(_factoryMock, dataProvider);

            synchronizer.UpdateBenchmarkWeight(_date);

            #endregion

            #region Assertions

            benchmarkMock.AssertWasCalled(x => x.SetComposition(Arg<DataTable>.Is.Anything, Arg<DateTime>.Is.Anything), o => o.Repeat.Times(1));

            #endregion
        }

        [Test(Description = "Test if the weight is being correctly calculated")]
        [TestCase(67123001, 67123003, true, 67123002, 123456, 12000000.0, 1.0, 10.0, 1.2, 1.0, 14400000.0, TestName = "[Float] FTSNOSH = 1.0 (same as no float)")]
        [TestCase(67123001, 67123003, true, 67123002, 123456, 12000000.0, 0.75, 10.0, 1.2, 1.0, 10800000.0, TestName = "[Float] FTSNOSH = 0.75")]
        [TestCase(67123001, 67123003, true, 67123002, 123456, 0.0, 0.75, 10.0, 1.2, 1.0, 9000000, TestName = "[Float] FTSNOSH = 0.0 (FTNOSH * 1,000,000.0)")]
        [TestCase(67123001, 67123003, false, 67123002, 123456, 0.0, 0.75, 10.0, 1.2, 1.0, 12000000, TestName = "[No Float] FTSNOSH = 0.0 (FTNOSH * 1,000,000.0)")]
        [TestCase(67123001, 67123003, true, 67123002, 123456, 12000000.0, 1.0, 0.0, 1.2, 1.0, 14400000.0, TestName = "[Float] FTNOSH = 0 (value is ignored)")]
        [TestCase(67123001, 67123003, true, 67123002, 123456, 12000000.0, 0.0, 0.0, 1.2, 1.0, 0.0, TestName = "[Float] FTSMCF = 0")]
        [TestCase(67123001, 67123003, false, 67123002, 123456, 12000000.0, 0.0, 0.0, 1.2, 1.0, 14400000.0, TestName = "[No Float] FTSMCF = 0")]
        [TestCase(67123001, 67123003, true, 67123002, 123456, 12000000.0, 1.0, 10.0, 0.0, 1.0, 0.0, TestName = "[Float] LAST = 0")]
        [TestCase(67123001, 67123003, true, 67123002, 123456, 12000000.0, 1.0, 10.0, 1.200123123, 1.0, 14401477.4760, TestName = "[Float] Decimal places")]
        [TestCase(67123001, 67123003, true, 67123002, 222222, 12000000.0, 0.75, 10.0, 1.2, 3.0, 32400000.0, TestName = "[Float] Test Foxex != 1.0")]
        [TestCase(67123001, 67123003, true, 67123002, 123456, 12000000.0, 0.75, 10.0, 1.2, 3.0, 10800000.0, TestName = "[Float] Test Forex = 1.0")]
        [TestCase(67123001, 67123003, false, 67123002, 222222, 12000000.0, 0.75, 10.0, 1.2, 3.0, 43200000.0, TestName = "[No Float] Test Foxex != 1.0")]
        [TestCase(67123001, 67123003, false, 67123002, 123456, 12000000.0, 0.75, 10.0, 1.2, 3.0, 14400000.0, TestName = "[No Float] Test Forex = 1.0")]
        public void TestBenchmarkWeightCalculation(int benchmark, int index, bool isFloat, int equity, int equityCurrency, decimal ftsnosh, decimal ftsmcf, decimal ftnosh, decimal last, double forex, decimal result)
        {
            #region Set up for the test scenario (Mocks and Stubs)

            var dataProvider = MockRepository.GenerateMock<IDataProvider>();
            var marketData = _factoryMock.GetCurrentMarketData();

            Assert.AreNotEqual(benchmark, equity);

            var benchmarkMock = MockRepository.GenerateMock<IBenchmarkVisitor>();
            var equityMock = MockRepository.GenerateMock<IEquityVisitor>();
            var indexMock = MockRepository.GenerateMock<IBasketVisitor>();

            benchmarkMock.Stub(x => x.GetComposition(Arg<DateTime>.Is.Anything)).Return(CreateComposition());
            benchmarkMock.Stub(x => x.Currency).Return(_factoryMock.StringToCurrency("GBP"));

            equityMock.Stub(x => x.Currency).Return(equityCurrency);
            equityMock.Stub(x => x.Sicovam).Return(equity);

            indexMock.Stub(x => x.Components).Return(new Collection<IEquityVisitor> {equityMock});

            marketData.Stub(x => x.GetForex(equityCurrency, benchmarkMock.Currency)).Return(forex);

            _factoryMock.Stub(x => x.GetInstrument(benchmark)).Do((Func<int, IInstrumentVisitor>)(x => benchmarkMock));
            _factoryMock.Stub(x => x.GetInstrument(equity)).Do((Func<int, IInstrumentVisitor>)(x => equityMock));
            _factoryMock.Stub(x => x.GetInstrument(index)).Do((Func<int, IInstrumentVisitor>)(x => indexMock));

            benchmarkMock.Stub(x => x.SetComposition(Arg<DataTable>.Is.Anything, Arg<DateTime>.Is.Anything))
                         .Do((Action<DataTable, DateTime>)((x, d) =>
                             {
                                 Assert.AreEqual(_date, d);
                                 Assert.AreEqual(1, x.Rows.Count);
                                 Assert.AreEqual(result, x.Rows[0][2]);
                                 Assert.AreEqual("Instrument", x.Rows[0][5]);
                             }));

            dataProvider.Stub(x => x.LoadEquityHistoric(Arg<int>.Is.Equal(equity), Arg<DateTime>.Is.Anything)).Return(new EquityHistoric
                {
                    Sicovam = equity,
                    Date = _date,
                    Ftsnosh = ftsnosh,
                    Ftsmcf = ftsmcf,
                    Ftnosh = ftnosh,
                    Last = last
                });

            dataProvider.Stub(x => x.LoadIndexToBenchmarkMappings()).Return(new List<BenchmarkUpdate>
                {
                    new BenchmarkUpdate
                        {
                            Sicovam = benchmark,
                            Index = index,
                            IsFloat = isFloat
                        }
                });

            #endregion

            #region The block of code that we want to test

            var synchronizer = new CSxSynchroniser(_factoryMock, dataProvider);

            synchronizer.UpdateBenchmarkWeight(_date);

            #endregion

            #region Assertions

            dataProvider.AssertWasCalled(x => x.LoadEquityHistoric(Arg<int>.Is.Equal(equity), Arg<DateTime>.Is.Anything), o => o.Repeat.Times(1));
            dataProvider.AssertWasCalled(x => x.LoadIndexToBenchmarkMappings(), o => o.Repeat.Times(1));
            benchmarkMock.AssertWasCalled(x => x.GetComposition(Arg<DateTime>.Is.Anything), o => o.Repeat.Times(1));
            benchmarkMock.AssertWasCalled(x => x.SetComposition(Arg<DataTable>.Is.Anything, Arg<DateTime>.Is.Anything), o => o.Repeat.Times(1));
            benchmarkMock.AssertWasCalled(x => x.Save(), o => o.Repeat.Times(1));

            #endregion
        }

        private static DataTable CreateComposition()
        {
            var composition = new DataTable();
            composition.Columns.Add("0", typeof (string));
            composition.Columns.Add("1", typeof (string));
            composition.Columns.Add("2", typeof (double));
            composition.Columns.Add("3", typeof (int));
            composition.Columns.Add("4", typeof (int));
            composition.Columns.Add("5", typeof (string));
            composition.Columns.Add("6", typeof (int));
            composition.Columns.Add("7", typeof (string));
            return composition;
        }
    }
}