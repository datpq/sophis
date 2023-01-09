using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mediolanum_RMA_FILTER.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichMarketAdapter.interfaces;
using RichMarketAdapter.ticket;
using sophis.log;
using transaction;

namespace Mediolanum_RMA_FILTER.Filters
{
    public class CSxRBCOrderFilter : IFilter
    {
        public bool filter(IMessageWrapper message)
        {
            using (var logger = new Logger("CSxRBCOrderFilter", MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "Begin");
                try
                {
                    ITicketMessage ticketMessage = (ITicketMessage) message.Message;
                    if (ticketMessage == null)
                    {
                        logger.log(Severity.warning, "ticketMessage cannot be cast as ITicketMessage. Discarding the message ...");
                        return false;
                    }
                    var orderRef = ticketMessage.getString(FieldId.ORDER_REFERENCE_PROPERTY_NAME);
                    string isPartialExecution = ticketMessage.getUserField("MEDIO_PARTIAL_EXECUTION");
                    if (isPartialExecution == "0")
                    {
                        ticketMessage.SetTicketField(FieldId.LAST_EXECUTION_PROPERTY_NAME, 1);
                        logger.log(Severity.debug, "Set last execution flag to true");
                    }
                    else
                        logger.log(Severity.debug, String.Format("order #{0} is a partial execution", orderRef));
                }
                catch (Exception e)
                {
                    logger.log(Severity.error, "Exception is thrown: " + e.Message);
                    logger.log(Severity.debug, "Stack trace : " + e.StackTrace);
                }
                logger.log(Severity.debug, "End");
            }
            return false;
        }
    }
}
