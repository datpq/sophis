using System;
using System.Collections.Generic;

using SophisETL.Common.Reporting;



namespace SophisETL.Common
{
    public interface IETLChain
    {
        string Name { get; set; }

        // Attach a new Reporting Handler to the Chain
        void AttachReportingHandler( IReportingHandler reportingHandler );

        // Delegates the Init methods to the Steps
        bool Init();

        // Delegates the Start methods to the Steps
        void Start();
    }
}
