using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SophisETL.Common.GlobalSettings;
using SophisETL.Common.ErrorMgt;



namespace SophisETL.Common
{
    public interface IETLStep
    {
        /// <summary>
        /// Step Name, primarily for logging references
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// In this method, the Step should verify that its setup is consistent, as much as possible
        /// This is called before starting the ETL Chain, and can abort the Start if required.
        /// </summary>
        void Init();

        /// <summary>
        /// In this method, the Step should execute the work to do
        /// </summary>
        void Start();
    }

    public interface IExtractStep : IETLStep
    {
        IETLQueue TargetQueue { get; set; }
    }

    public interface ITransformStep : IETLStep
    {
        List<IETLQueue> SourceQueues { get; }
        List<IETLQueue> TargetQueues { get; }
    }

    public interface ILoadStep : IETLStep
    {
        IETLQueue SourceQueue { get; set; }

        // Event sent when a record was successfully loaded
        event RecordLoadedEventHandler RecordLoaded;
        // Event sent when a record was unsuccessfully loaded
        event RecordNotLoadedEventHandler RecordNotLoaded;
    }
}
