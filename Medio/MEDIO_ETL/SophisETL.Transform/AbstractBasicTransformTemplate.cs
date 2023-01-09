using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SophisETL.Common;
using SophisETL.Common.Logger;
using SophisETL.Common.ErrorMgt;



namespace SophisETL.Transform
{
    /// <summary>
    /// The Basic Transform template provides a default implementation of a basic transform step:
    /// - 1 and only 1 source queue, accessible through the SourceQueue property
    /// - 1 and only 1 target queue, accessible through the TargetQueue property
    /// 
    /// You must implement the abstract methods:
    /// - Record Transform(Record input) - transformation of 1 record
    /// 
    /// You can override:
    /// - The Init() method to add you own initializations and checks, but callback the parent method!
    /// 
    /// </summary>
    public abstract class AbstractBasicTransformTemplate : ITransformStep
    {
        // Internal members
        private int _RecordsTransformedCount = 0;





        #region ITransformStep Members

        //---- Forces One Source / One Target - available through SourceQueue and Target Queue ----
        public List<IETLQueue> SourceQueues { get { return _SourceQueues; } }
        private List<IETLQueue> _SourceQueues = new List<IETLQueue>();

        public List<IETLQueue> TargetQueues { get { return _TargetQueues; } }
        private List<IETLQueue> _TargetQueues = new List<IETLQueue>();

        // We have an Input and a Target Queue aliased to the List when we have only one
        public IETLQueue SourceQueue { get { return SourceQueues[0]; } set { SourceQueues.Clear(); SourceQueues.Add( value ); } }
        public IETLQueue TargetQueue { get { return TargetQueues[0]; } set { TargetQueues.Clear(); TargetQueues.Add( value ); } }
        //-----------------------------------------------------------------------------------------

        #endregion

        #region IETLStep Members

        private string _Name;
        public string Name { get { return _Name; } set { _Name = value; } }

        public virtual void Init()
        {
            // Make sure we have only 1 Source / Target Queue
            if ( _SourceQueues.Count != 1 )
                throw new Exception( "Transform step [" + Name + "/" + this.GetType().Name + "] must have one and only one Source Queue" );
            if ( _TargetQueues.Count != 1 )
                throw new Exception( "Transform step [" + Name + "/" + this.GetType().Name + "] must have one and only one Source Queue" );
        }

        public void Start()
        {
            while ( SourceQueue.HasMore )
            {
                Record record = SourceQueue.Dequeue();
                if ( record != null )
                {
                    try
                    {
                        TargetQueue.Enqueue( Transform( record ) );
                        _RecordsTransformedCount++;
                    }
                    catch ( Exception ex )
                    {
                        // Enrich the exception and pass it to the Error Handler
                        ErrorHandler.Instance.HandleError( new ETLError()
                        {
                            Step = this,
                            Exception = ex,
                            Record = record,
                            Message = ex.Message,
                        } );
                    }
                }
            }

            LogManager.Instance.Log( "Transform/Basic/" + Name + ": step finished - "
                    + _RecordsTransformedCount + " record(s) transformed" );
        }

        #endregion


        /// <summary>
        /// Implement your own Record Transformation in this method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        protected abstract Record Transform( Record record );
    }
}
