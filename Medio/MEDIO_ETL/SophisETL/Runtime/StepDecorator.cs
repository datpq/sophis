using System;
using System.Collections.Generic;
using System.Text;

using SophisETL.Common;
using SophisETL.Common.ErrorMgt;
using SophisETL.Common.Logger;


namespace SophisETL.Runtime
{
    /// <summary>
    /// This decorator handles some common aspects of the Steps:
    /// - Handling of top level exceptions
    /// - Reporting on Start and Stop events
    /// </summary>
    internal class StepDecorator
    {
        private IETLStep _DecoratedStep;

        /// <summary>
        /// Event triggered when a step has a General Error
        /// The receiving end of this event should stop all processing since records
        /// data integrity can no longer be maintained
        /// </summary>
        public event EventHandler<GeneralErrorEventArgs> GeneralErrorOnStep;

        public StepDecorator( IETLStep decoratedStep )
        {
            _DecoratedStep = decoratedStep;
        }

        public void Start()
        {
            try
            {
                // Also handle the decorative logging
                string stepFullName = String.Format( "[{0}/{1}/{2}]",
                    _DecoratedStep is ILoadStep ? "Load" : _DecoratedStep is ITransformStep ? "Transform" : "Extract",
                    _DecoratedStep.GetType().Name,
                    _DecoratedStep.Name );

                LogManager.Instance.Log( stepFullName + " Starting Step..." );
                _DecoratedStep.Start();
                LogManager.Instance.Log( stepFullName + " Step completed" );
            }
            catch ( Exception ex )
            {
                // Enrich the exception
                ETLError error = new ETLError()
                {
                    Step = _DecoratedStep,
                    Exception = ex,
                    Message = ex.Message,
                };
                // And pass it to the Error Handler (logs, etc...)
                ErrorHandler.Instance.HandleError( error );

                // Notify the Chain of a General Error
                if ( GeneralErrorOnStep != null )
                    GeneralErrorOnStep(this, new GeneralErrorEventArgs { FailedStepName = _DecoratedStep.Name } );

                //// Warn the target queues that the job is over for this one..
                //// v1.5.1,2011-02-23,AdB: actually the General Error event should abort all processing
                //// just doing as if nothing happened is too dangerous (true-ups will assume that one of the side
                //// is zero for example)
                //if ( _DecoratedStep is IExtractStep )
                //{
                //    ( _DecoratedStep as IExtractStep ).TargetQueue.RemoveProducer( _DecoratedStep as IProducer );
                //}
                //else if ( _DecoratedStep is ITransformStep )
                //{
                //    foreach ( IETLQueue queue in ( _DecoratedStep as ITransformStep ).TargetQueues )
                //        queue.RemoveProducer( _DecoratedStep as IProducer );
                //}
            }
        }
    }

    /// <summary>
    /// Data passed on a General Error event
    /// </summary>
    internal class GeneralErrorEventArgs : EventArgs
    {
        public string FailedStepName { get; set; }
    }
}
