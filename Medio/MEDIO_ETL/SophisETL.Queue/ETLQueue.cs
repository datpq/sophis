using System;
using System.Collections.Generic;
using System.Threading;

using SophisETL.Common;


namespace SophisETL.Queue
{
    internal class ETLQueueBasic : IETLQueue
    {
        // Members
        private Queue<Record> _InternalQueue = new Queue<Record>();
        readonly object _lock = new object();
      
        bool _queueHasMore = true;

        // Properties
        public string Name { get { return _Name; } }
        private string _Name;

        // Opened is: Producers are open or we still have some records pending
        public bool HasMore 
        {
            get { return _queueHasMore; }
        }
 
        // constructor
        public ETLQueueBasic( string name )
        {
            _Name = name;
        }

        //// Accessible methods

        public void Enqueue(Record record)
        {
            _InternalQueue.Enqueue( record );
        }

        /// <summary>
        /// Returns the next available Record (blocking)
        /// Can return null if there is no more records to be produced by this queue
        /// </summary>
        /// <returns></returns>
        public Record Dequeue()
        {
            Record record = null;

            if ( _InternalQueue.Count > 0 )
                record = _InternalQueue.Dequeue();

            // Update the Queue Has More flag
            if ( _InternalQueue.Count == 0)
                _queueHasMore = false;
           
            return record;
        }

        ///// <summary>
        ///// Register a new Producer
        ///// </summary>
        ///// <param name="producer"></param>
        //public void RegisterProducer( IProducer producer )
        //{
        //    // Add it to the List of Active Producers and register to its "NoMoreRecords" Event
        //    _ActiveProducers.Add( producer );
        //    producer.NoMoreRecords += new EventHandler( ProducerHasNoMoreRecords );
        //}

        ///// <summary>
        ///// Register a Consumer of this queue
        ///// (actually just make sure that we have only one consumer registered!)
        ///// </summary>
        ///// <param name="consumer"></param>
        //public void RegisterConsumer( IConsumer consumer )
        //{
        //    if ( _ConsumerRegistered )
        //    {
        //        throw new Exception( "Queue " + Name + " can not have more than 1 consumer attached!" );
        //    }
        //    _ConsumerRegistered = true;
        //}
        //private bool _ConsumerRegistered = false;

        //public void ProducerHasNoMoreRecords( object producer, EventArgs e )
        //{
        //    if ( producer is IProducer )
        //        RemoveProducer( producer as IProducer );
        //}

        //public void RemoveProducer( IProducer producer )
        //{
        //    _ActiveProducers.Remove( producer as IProducer );

        //    Enqueue( null ); // terminating Null record to force wake up of consuming thread
        //    //SophisETL.Common.Logger.LogManager.Instance.LogDebug("Producer completed, number of active producers remaining: " + _ActiveProducers.Count);
        //}
    }
}
