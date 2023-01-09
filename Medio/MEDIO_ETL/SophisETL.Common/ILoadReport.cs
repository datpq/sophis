using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SophisETL.Common
{
 
    // Implement one of those
    public delegate void RecordLoadedEventHandler   (ILoadStep loader, Record record, ILoadSuccessReport report);
    public delegate void RecordNotLoadedEventHandler(ILoadStep loader, Record record, ILoadFailureReport report);

    // Load was successfull
    public interface ILoadSuccessReport
    {
        string SuccessMessage { get; }
    }

    public interface ILoadFailureReport
    {
        string FailureMessage { get; }
        Exception FailureException { get; }
    }
}
