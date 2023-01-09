// -----------------------------------------------------------------------
//  <copyright file="TestSophisFactory.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/30</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.Tests.Toolkit
{
    using System;
    using Auditing;
    using Castle.MicroKernel;
    using Castle.Windsor;
    using NUnit.Framework;
    using Rhino.Mocks;
    using StaticData.Impl;
#if V421x
    using Castle.Core.Interceptor;
#else
    using Castle.DynamicProxy;

#endif

    [TestFixture]
    public class TestSophisFactory
    {

        [TearDown]
        public void TestCleanUp()
        {
            //SophisContainer.Container = new WindsorContainer();
        }

        [SetUp]
        public void TestSetup()
        {
            try
            {
                SophisContainer.RegisterSingleton<AuditingInterceptor>();

                SophisContainer.Register<ITestInterface, TestClass>("Test");
                SophisContainer.RegisterInstance<ITestInterface>("Test2", new TestClass("with value"));
                SophisContainer.RegisterInstance<ITestInterface>("Test3", MockRepository.GenerateMock<TestClass>("something"));
                SophisContainer.Register<ITestInterface, TestClass>("Test4");

                SophisContainer.RegisterSingleton<OussInterceptor>();
            }
            catch (ComponentRegistrationException)
            {
            }
        }

        [Test]
        public void TestUnnamedResolver()
        {
            var c = SophisContainer.Resolve<ITestInterface>(new object[] {"someshit"});
            c.Method("test");
        }

        [Test]
        public void TestInstanceResolver()
        {
            var c = SophisContainer.Resolve<ITestInterface>("Test2");
            var s = c.Method("test");
            Assert.AreEqual("with value", s);
        }

        [Test]
        public void TestMockedInstanceResolver()
        {
            var c = SophisContainer.Resolve<ITestInterface>("Test3");
            var s = c.Method("test");
            Assert.AreEqual("something", s);
        }

        [Test]
        public void TestNamedResolver()
        {
            var c = SophisContainer.Resolve<ITestInterface>("Test4", "wow");
            var s = c.Method("test");
            Assert.AreEqual("wow", s);
        }
    }

    public class TestClass : ITestInterface
    {
        private readonly string _param;

        public TestClass(string param)
        {
            _param = param;
        }

        public string Method(string param)
        {
            Console.WriteLine("Doing Something [{1}] : {0}", param, _param);
            return _param;
        }
    }

    public interface ITestInterface
    {
        string Method(string param);
    }

    public class OussInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            Console.WriteLine("Oussama was here!!!");
            invocation.Proceed();
        }
    }
}