using System;
using System.Collections.Generic;

using SophisETL.Common;



namespace SophisETL.Common.ErrorMgt
{
    public class ETLError
    {
        public IETLStep  Step      { get; set; }
        public Record    Record    { get; set; } // can be null
        public string    Message   { get; set; }
        public Exception Exception { get; set; } // can be null
    }
}
