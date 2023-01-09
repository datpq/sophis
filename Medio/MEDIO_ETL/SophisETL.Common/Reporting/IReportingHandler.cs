using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SophisETL.Common.ErrorMgt;


namespace SophisETL.Common.Reporting
{
    /// <summary>
    /// Describes a class capable of handling the Chain Reporting
    /// Decorate the class with a SettingsTypeAttribute to be injected with custom settings at run time
    /// </summary>
    public interface IReportingHandler
    {
        string Name { get; set; }

        void Init();

        void ReportETLError( ETLError error );
        void HandleRecordLoadedEvent( ILoadStep loader, Record record, ILoadSuccessReport report );
        void HandleRecordNotLoadedEvent( ILoadStep loader, Record record, ILoadFailureReport report );


        void Dispose();
    }
}
