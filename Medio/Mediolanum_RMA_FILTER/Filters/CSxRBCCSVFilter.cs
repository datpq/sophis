using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using Mediolanum_RMA_FILTER.Data;
using Mediolanum_RMA_FILTER.Interfaces;
using Mediolanum_RMA_FILTER.Tools;
using RichMarketAdapter.interfaces;
using RichMarketAdapter.ticket;
using sophis.log;

namespace Mediolanum_RMA_FILTER.Filters
{
    public class CSxRBCCSVFilter : IFilter
    {
        private static string _ClassName = typeof(CSxRBCCSVFilter).Name; 
        private static bool _isInitialized = false;

        /// <summary>
        /// Workaround due to issues on the LMInterface initialization. Should be reverted before going live.
        /// </summary>
        private void Initialize()
        {
            if (!_isInitialized)
            {
                RBCCustomParameters.Instance.LogCustomParameters();
                this.CacheData();
                _isInitialized = true;
            }
        }

        public virtual bool filter(IMessageWrapper message)
        {
            Initialize();
            using (var logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {

                bool reject = false; // If true, the message will not be sent to IS 
                try
                {
                    if (message == null)
                    {
                        logger.log(Severity.warning, "Received message is null. Discarding the message ...");
                        return true;
                    }
                    ITicketMessage ticketMessage = (ITicketMessage) message.Message;
                    if (ticketMessage == null)
                    {
                        logger.log(Severity.warning, "ticketMessage cannot be cast as ITicketMessage. Discarding the message ...");
                        return true;
                    }
                    logger.log(Severity.info, "Start processing ticketMessage ...");
                    reject = RBCTicketCreatorFactory.CreateTicketMessage(ref ticketMessage, ticketMessage.TextData.SplitCSV());
                }
                catch (Exception e)
                {
                    logger.log(Severity.error, "Exception is thrown: " + e.Message);
                    logger.log(Severity.debug, "Stack trace : " + e.StackTrace);
                    return reject; 
                }

                string msg = !reject
                    ? "TicketMessage processed!"
                    : "TicketMessage has been rejected! Will not be processed by IS";

                logger.log(Severity.info, msg);
                return reject;
            }
        }

        private void CacheData()
        {
            CSxCachingDataManager.Instance.AddItem(eMedioCachedData.Rootportfolios.ToString(), CSxRBCHelper.GetAccountList());
            CSxCachingDataManager.Instance.AddItem(eMedioCachedData.Delegatemanagers.ToString(), CSxRBCHelper.GetDelegateManagers());
            CSxCachingDataManager.Instance.AddItem(eMedioCachedData.Businessevents.ToString(), CSxRBCHelper.GetBusinessEventsList());
        }

    }
}
