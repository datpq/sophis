using System;
using System.Collections.Generic;
using System.Threading;

using SophisETL.Common;


namespace SophisETL.Common
{
    // An ETL Queue is a Synchronized Queue Collection where records processed are pushed waiting to be consumed
    // Our ETL Queue, contains "Record" type objects
    //
    // All Queues must be constructed by the Queue Manager (and accessed from there)
    //
    // Only the interface should be public
    public interface IETLQueue
    {
        string Name { get; }

        /// <summary>
        /// True as long as the queue can produce more records
        /// </summary>
        bool HasMore { get; }

        void Enqueue( Record record );
        Record Dequeue();

        ///// <summary>
        ///// The Queue is notified that a new Producer has been connected
        ///// </summary>
        ///// <param name="producer"></param>
        //void RegisterProducer( IProducer producer );

        ///// <summary>
        ///// The Queue is notified that a new Consumer has been connected
        ///// </summary>
        ///// <param name="producer"></param>
        //void RegisterConsumer( IConsumer consumer );


        ///// <summary>
        ///// The Queue is notified that a Producer is removed
        ///// </summary>
        ///// <param name="producer"></param>
        //void RemoveProducer( IProducer iProducer );
    }
}
