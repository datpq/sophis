// -----------------------------------------------------------------------
//  <copyright file="SophisContainer.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/02/07</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit.StaticData.Impl
{
    using System;
    using System.Linq;
    using Auditing;
    using Castle.DynamicProxy;
    using Castle.MicroKernel;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Instruments;
    using Instruments.Impl;

    public static class SophisContainer
    {
        private static readonly IWindsorContainer Container = new WindsorContainer();
        private static bool _registered;

        //private static IWindsorContainer Container
        //{
        //    get { return _container; }
        //    //set { _container = value; }
        //}

        public static void Register<TI, TC>(string name) where TC : TI
        {
            if (!typeof (TI).IsInterface)
                throw new ArgumentException("TI type must be an Interface");

            Type[] i = Container.ResolveAll<IInterceptor>().Select(x => x.GetType()).ToArray();

            Container.Register(Component.For<TI>().ImplementedBy<TC>().Named(name).LifeStyle.Transient.Interceptors(i));
        }

        public static void Register<TI, TC>() where TC : TI
        {
            if (!typeof (TI).IsInterface)
                throw new ArgumentException("TI type must be an Interface");

            Type[] i = Container.ResolveAll<IInterceptor>().Select(x => x.GetType()).ToArray();

            Container.Register(Component.For<TI>().ImplementedBy<TC>().LifeStyle.Transient.Interceptors(i));
        }

        public static void Register<T>() where T : class
        {
            Type[] i = Container.ResolveAll<IInterceptor>().Select(x => x.GetType()).ToArray();

            Container.Register(Component.For<T>().LifeStyle.Transient.Interceptors(i));
        }

        public static void RegisterInstance<TI>(TI instance) where TI : class
        {
            if (!typeof (TI).IsInterface)
                throw new ArgumentException("TI type must be an Interface");

            Type[] i = Container.ResolveAll<IInterceptor>().Select(x => x.GetType()).ToArray();

            Container.Register(Component.For<TI>().Instance(instance).Interceptors(i));
        }

        public static void RegisterInstance<TI>(string name, TI instance) where TI : class
        {
            if (!typeof (TI).IsInterface)
                throw new ArgumentException("TI type must be an Interface");

            IInterceptor[] i = Container.ResolveAll<IInterceptor>().ToArray();

            var pg = new ProxyGenerator();
            TI proxy = pg.CreateInterfaceProxyWithTarget(instance, i);

            Container.Register(Component.For<TI>().Instance(proxy).Named(name));
        }

        public static void RegisterSingleton<T>() where T : class
        {
            Container.Register(Component.For<T>().LifeStyle.Singleton);
        }

        public static T Resolve<T>(params object[] args) where T : class
        {
            var component = Container.Resolve<T>(new Arguments(args));

            if (component == null)
                throw new ArgumentOutOfRangeException(string.Format("No component found for : {0}", typeof (T)));

            return component;
        }

        public static T Resolve<T>(string name, params object[] args) where T : class
        {
            var component = Container.Resolve<T>(name, new Arguments(args));

            if (component == null)
                throw new ArgumentOutOfRangeException(string.Format("No component found for : {0}", typeof (T)));

            return component;
        }

        public static T[] ResolveAll<T>(params object[] args) where T : class
        {
            T[] component = Container.ResolveAll<T>(new Arguments(args));

            if (component == null)
                throw new ArgumentOutOfRangeException(string.Format("No components found for : {0}", typeof (T)));

            return component;
        }

        public static object Resolve(string name, params object[] args)
        {
            object component = Container.Resolve(name, new Arguments(args));

            if (component == null)
                throw new ArgumentOutOfRangeException(string.Format("No component found with name : {0}", name));

            return component;
        }

        public static void Release(object instance)
        {
            Container.Kernel.ReleaseComponent(instance);
        }

        public static void RegisterOnce()
        {
            if (_registered) return;
            _registered = true;

            //Registers itself
            Register<ISophisFactory, SophisFactory>();

            //Registers Interceptors
            RegisterSingleton<AuditingInterceptor>();

            //Registers Intruments

            /*
            A - Share
            B - Cap and Floor
            C - Commission
            D - Derivative
            E - Forex pair
            F - Future
            G - Contracts for difference
            H - Issuer
            I - Basket & Index
            K - Non Deliverable Forex Forward
            L - Repo
            M - Listed option
            N - Package
            O - Bond
            P - Stock-Loan
            Q - Commodity
            R - Rate
            S - Swap
            T - Debt instrument
            U - Commodity basket
            W - Swaped option
            X - Forex forward
            Y - Benchmark
            Z - Internal Fund
            */
            Register<IEquityVisitor, EquityVisitor>("A");
            Register<IBondVisitor, BondVisitor>("O");
            Register<IBenchmarkVisitor, BenchmarkVisitor>("Y");
            Register<IBasketVisitor, BasketVisitor>("I");
        }
    }
}