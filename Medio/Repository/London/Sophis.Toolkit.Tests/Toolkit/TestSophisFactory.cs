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
    using System.Linq;
    using System.Reflection;
    using Aop;
    using Aop.Auditing;
    using Castle.MicroKernel;
    using NUnit.Framework;
    using Rhino.Mocks;
    using StaticData.Impl;
#if V421x
    using Castle.Core.Interceptor;
#else
    using Castle.DynamicProxy;
#endif
    using Utils.Log;

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

                SophisContainer.RegisterSingleton<ToolkitInterceptor>();
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

        [Test]
        public void TestArrayResolver()
        {
            var c = SophisContainer.ResolveAll<ITestInterface>("wow");
            Assert.Greater(c.Length, 1);

            foreach (var t in c)
            {
                var s = t.Method("test");
                Assert.IsNotEmpty(s);
            }
        }

        [Test]
        public void TestAuditingAttribute()
        {

            var c = SophisContainer.Resolve<ITestInterface>(new object[]{"wowcha"});
            var s1 = c.Method("test");
            var s2 = c.MethodWithLog("test");
            Assert.AreEqual(s1, s2);

            LogHelper.Prototype.AssertWasCalled(x => x.WriteDebug(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything), o=>o.Repeat.Times(2));
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

        public string MethodWithLog(string param)
        {
            return Method(param);
        }
    }

    public interface ITestInterface
    {
        string Method(string param);
        [Log]
        [Toolkit(Desc = "Obama")]
        string MethodWithLog(string param);
    }

    public class ToolkitAttribute : SophisAttribute
    {
        public string Desc { get; set; }
    }

    [Toolkit]
    public class ToolkitInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            using (var log = LogHelper.GetLogger(GetType().Name, MethodBase.GetCurrentMethod().Name))
            {
                var  attribs = invocation.Method.GetCustomAttributes(true);
                var attrib = attribs.FirstOrDefault(x => x.GetType() == typeof(ToolkitAttribute)) as ToolkitAttribute;
                if (attrib != null)
                {
                    log.WriteInfo("Toolkit team was here : {0}", attrib.Desc);
                }

                invocation.Proceed();
            }
        }
    }
}