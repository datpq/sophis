// -----------------------------------------------------------------------
//  <copyright file="Class1.cs" company="Sophis Tech (Toolkit)">
//  Copyright (c) Sophis Tech. All rights reserved.
//  </copyright>
//  <author>Marco Cordeiro</author>
//  <created>2013/01/29</created>
// -----------------------------------------------------------------------

namespace Sophis.Toolkit
{
    using System;
    using BritishSteel;
    using BritishSteel.Data;
    using StaticData.Impl;
    using sophis;

    public class ImplAssemblyEntrypoint : IMain
    {
        public void EntryPoint()
        {
            //{{SOPHIS_INITIALIZATION (do not delete this line)

            // TO DO; Perform registrations
            SophisContainer.RegisterOnce();

            SophisContainer.Register<IDataProvider, CSxDataProvider>();
            SophisContainer.Register<CSxSynchroniser>();

            sophis.scenario.CSMScenario.Register("Calculate Benchmarks", new CSxSyncScenario());
            //}}SOPHIS_INITIALIZATION
        }

        public void Close()
        {
            GC.Collect();
        }
    }
}