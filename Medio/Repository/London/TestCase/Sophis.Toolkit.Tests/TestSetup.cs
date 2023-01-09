// -----------------------------------------------------------------------
//  <copyright file="TestSetup.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/29</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Tests
{
    using System;
    using NUnit.Framework;
    using Rhino.Mocks;
    using Utils.Log;

    [SetUpFixture]
    public class TestSetup
    {
        [TearDown]
        public void Cleanup()
        {
            LogHelper.Prototype = null;
        }

        [SetUp]
        public void Setup()
        {
            var tName = string.Empty;
            var mName = string.Empty;
            var logger = MockRepository.GenerateMock<ICSxLog>();
            LogHelper.Prototype = logger;
            logger.Stub(x => x.Clone()).Return(logger);
            logger.Stub(x => x.Begin(Arg<string>.Is.Anything, Arg<string>.Is.Anything))
                  .Do((Action<string, string>) ((t, m) =>
                      {
                          tName = m; mName = m;
                      }));
            logger.Stub(x => x.WriteDebug(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
                  .Do((Action<string, object[]>) ((m, a) => Console.WriteLine("{0}.{1} [Debug] : {2}", tName, mName, string.Format(m, a))));
            logger.Stub(x => x.WriteError(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
                  .Do((Action<string, object[]>) ((m, a) => Console.WriteLine("{0}.{1} [Error] : {2}", tName, mName, string.Format(m, a))));
            logger.Stub(x => x.WriteInfo(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
                  .Do((Action<string, object[]>) ((m, a) => Console.WriteLine("{0}.{1} [Info] : {2}", tName, mName, string.Format(m, a))));
            logger.Stub(x => x.WriteVerbose(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything))
                  .Do((Action<string, object[]>) ((m, a) => Console.WriteLine("{0}.{1} [Verbose] : {2}", tName, mName, string.Format(m, a))));

        }
    }
}