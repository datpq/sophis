using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SophisETL.Common;
//using System.Diagnostics;

namespace SophisETL.Queue
{
    // Handle all the available Queues in the ETL Process
    public class ETLQueueManager
    {
        // Managed Queues
        private Dictionary<string, IETLQueue> _ManagedQueues;

        //// Injectable Dependencies
        // Queue Factory
        private ETLQueueFactory _QueueFactory;

        
        //// Accessible Methods

        // Access to an existing queue, or create one if it does not exist
        public IETLQueue GetQueue( string name )
        {
//#if DEBUG
//            Debugger.Launch();
//#endif
            if ( !_ManagedQueues.ContainsKey( name ) )
            {
                IETLQueue newQueue = _QueueFactory.CreateQueue( name );
                _ManagedQueues.Add( name, newQueue );
            }
            return _ManagedQueues[ name ];
        }

        public void CleanQueues()
        {
            _ManagedQueues = new Dictionary<string, IETLQueue>();
        }


        // Singleton mode
        private static ETLQueueManager _Instance = null;
        public  static ETLQueueManager  Instance
        {
            get
            {
                if ( null == _Instance )
                    _Instance = new ETLQueueManager();
                return _Instance;
            }
        }
        private ETLQueueManager()
        {
            _ManagedQueues = new Dictionary<string, IETLQueue>();
        }

        public void Init()
        {
            // Create the Queue Factory
            _QueueFactory = new ETLQueueFactory();
        }
    }
}
