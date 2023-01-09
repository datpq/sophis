using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SophisETL.Common;

namespace SophisETL.Queue
{
    // Class in charge of creating the proper kind of Queue (depending on its name and the setup)
    internal class ETLQueueFactory
    {
        public IETLQueue CreateQueue( string name )
        {
            // TODO: use Queue definition XML file to set specific settings and allow user custom queues
            return new ETLQueueBasic( name );
        }
    }
}
