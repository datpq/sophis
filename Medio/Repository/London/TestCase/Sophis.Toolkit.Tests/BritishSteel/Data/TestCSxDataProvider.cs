// -----------------------------------------------------------------------
//  <copyright file="Class1.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/25</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Tests.BritishSteel.Data
{
    using System;
    using System.Data.Common;
    using NUnit.Framework;
    using Rhino.Mocks;
    using Sophis.Toolkit.BritishSteel.Data;

    [TestFixture]
    public class TestCSxDataProvider
    {
        private DbDataReader _reader;
        private CSxDataProvider _dataProvider;

        [TearDown]
        public void TestCleanUp()
        {
            //SophisContainer.Container = new WindsorContainer();
        }

        [SetUp]
        public void TestSetup()
        {
            _dataProvider = MockRepository.GenerateMock<CSxDataProvider>();
            //SophisContainer.RegisterInstance<IDataProvider>(mock);

            _reader = MockRepository.GenerateMock<DbDataReader>();
            _dataProvider.Stub(x => x.GetDbDataReader(Arg<string>.Is.Anything, Arg<string[]>.Is.Anything, Arg<object[]>.Is.Anything))
                .Do((Func<string, string[], object[], DbDataReader>) ((x1, x2, x3) => _reader));
        }

        [Test(Description = "Test if the benchmark is loading correctly")]
        [TestCase(67123001, 67123002, false, 67123001, 67123002, false, TestName = "isfloat = bool [false]")]
        [TestCase(67123001, 67123002, 1, 67123001, 67123002, true, TestName = "isfloat is number [=1 => true]")]
        [TestCase(67123001, 67123002, 0, 67123001, 67123002, false, TestName = "isfloat is number [=0 => false]")]
        [TestCase(67123001, 67123002, 3, 67123001, 67123002, true, TestName = "isfloat is number [!=0 => true]")]
        [TestCase(null, null, null, 0, 0, false, TestName = "null values")]
        [TestCase(67123001, 67123002, true, 67123001, 67123002, true, TestName = "isfloat = bool [true]")]
        public void TestBenchmarkLoading(object sicovam, object index, object isFloat, int expectedSicovam, int expectedindex, bool expectedIsFloat)
        {
            #region Set up for the test scenario (Mocks and Stubs)

            //var dataProvider = SophisContainer.Resolve<IDataProvider>();
            var counter = 0;
            _reader.Stub(x => x.Read()).Do((Func<bool>) (() => counter++ < 1));
            _reader.Stub(x => x.GetOrdinal("BENCHMARK")).Return(0);
            _reader.Stub(x => x.GetOrdinal("BASKET")).Return(1);
            _reader.Stub(x => x.GetOrdinal("IS_FLOAT")).Return(2);
            _reader.Stub(x => x.GetValue(0)).Return(sicovam ?? DBNull.Value);
            _reader.Stub(x => x.GetValue(1)).Return(index ?? DBNull.Value);
            _reader.Stub(x => x.GetValue(2)).Return(isFloat ?? DBNull.Value);

            #endregion

            #region The block of code that we want to test

            var result = _dataProvider.LoadIndexToBenchmarkMappings();

            #endregion

            #region Assertions

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expectedSicovam, result[0].Sicovam);
            Assert.AreEqual(expectedindex, result[0].Index);
            Assert.AreEqual(expectedIsFloat, result[0].IsFloat);

            _reader.AssertWasCalled(x => x.GetOrdinal("BENCHMARK"), o => o.Repeat.Times(1));
            _reader.AssertWasCalled(x => x.GetOrdinal("BASKET"), o => o.Repeat.Times(1));
            _reader.AssertWasCalled(x => x.GetOrdinal("IS_FLOAT"), o => o.Repeat.Times(1));
            _reader.AssertWasCalled(x => x.GetValue(Arg<int>.Is.Anything), o => o.Repeat.Times(3));

            #endregion
        }

        [Test(Description = "Test if the historic is loading correctly")]
        [TestCase(67123102, 1000.0, 1.0, 10.0, 1.1, 67123102, 1000.0, 1.0, 10.0, 1.1, TestName = "Random Values")]
        [TestCase(null, null, null, null, null, 0, 0.0, 1.0, 0.0, 0.0, TestName = "Null values")]
        public void TestHistoricLoading(object sicovam, object ftsnosh, object ftsmcf, object ftnosh, object last, int expectedSicovam, decimal expectedFtsnosh, decimal expectedFtsmcf, decimal expectedFtnosh, decimal expectedLast)
        {
            #region Set up for the test scenario (Mocks and Stubs)

            //var dataProvider = SophisContainer.Resolve<IDataProvider>();

            _reader.Stub(x => x.Read()).Do((Func<bool>)(() => true));

            _reader.Stub(x => x.GetOrdinal("SICOVAM")).Return(0);
            _reader.Stub(x => x.GetOrdinal("JOUR")).Return(1);
            _reader.Stub(x => x.GetOrdinal("FTSNOSH")).Return(2);
            _reader.Stub(x => x.GetOrdinal("FTSMCF")).Return(3);
            _reader.Stub(x => x.GetOrdinal("FTNOSH")).Return(4);
            _reader.Stub(x => x.GetOrdinal("LAST")).Return(5);

            _reader.Stub(x => x.GetValue(0)).Return(sicovam);
            _reader.Stub(x => x.GetValue(1)).Return(DateTime.Today);
            _reader.Stub(x => x.GetValue(2)).Return(ftsnosh);
            _reader.Stub(x => x.GetValue(3)).Return(ftsmcf);
            _reader.Stub(x => x.GetValue(4)).Return(ftnosh);
            _reader.Stub(x => x.GetValue(5)).Return(last);

            #endregion

            #region The block of code that we want to test

            var result = _dataProvider.LoadEquityHistoric(67123102, DateTime.Today);

            #endregion

            #region Assertions

            Assert.AreEqual(expectedSicovam, result.Sicovam);
            Assert.AreEqual(DateTime.Today, result.Date);
            Assert.AreEqual(expectedFtsnosh, result.Ftsnosh);
            Assert.AreEqual(expectedFtnosh, result.Ftnosh);
            Assert.AreEqual(expectedFtsmcf, result.Ftsmcf);
            Assert.AreEqual(expectedLast, result.Last);

            _reader.AssertWasCalled(x => x.GetOrdinal("SICOVAM"), o => o.Repeat.Times(1));
            _reader.AssertWasCalled(x => x.GetOrdinal("JOUR"), o => o.Repeat.Times(1));
            _reader.AssertWasCalled(x => x.GetOrdinal("FTSNOSH"), o => o.Repeat.Times(1));
            _reader.AssertWasCalled(x => x.GetOrdinal("FTSMCF"), o => o.Repeat.Times(1));
            _reader.AssertWasCalled(x => x.GetOrdinal("FTNOSH"), o => o.Repeat.Times(1));
            _reader.AssertWasCalled(x => x.GetOrdinal("LAST"), o => o.Repeat.Times(1));

            _reader.AssertWasCalled(x => x.GetValue(Arg<int>.Is.Anything), o => o.Repeat.Times(6));

            #endregion
        }
        
        [Test, ExpectedException("System.IO.InvalidDataException")]
        public void TestHistoricLoadingWithNoData()
        {
            #region Set up for the test scenario (Mocks and Stubs)

            //var dataProvider = SophisContainer.Resolve<IDataProvider>();

            _reader.Stub(x => x.Read()).Do((Func<bool>)(() => false));

            #endregion

            #region The block of code that we want to test

            _dataProvider.LoadEquityHistoric(67123102, DateTime.Today);

            #endregion
        }
    }
}