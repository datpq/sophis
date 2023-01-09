using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mediolanum_RMA_FILTER.Interfaces;
using Mediolanum_RMA_FILTER.TicketCreator;
using Mediolanum_RMA_FILTER.Tools;
using RichMarketAdapter.interfaces;
using RichMarketAdapter.ticket;
using sophis.log;
using transaction;

namespace Mediolanum_RMA_FILTER.Filters
{
    public class CSxRBCCAMergerFilter : CSxRBCCSVFilter
    {
        private static string _ClassName = typeof(CSxRBCCAMergerFilter).Name;

        public CSxRBCCAMergerFilter() : base()
        {
        }

        public override bool filter(IMessageWrapper message)
        {
            using (var logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    if (message == null)
                    {
                        logger.log(Severity.warning, "Received message is null. Discarding the message ...");
                        return true;
                    }
                    ITicketMessage ticketMessage = (ITicketMessage)message.Message;
                    if (ticketMessage == null)
                    {
                        logger.log(Severity.warning, "ticketMessage cannot be cast as ITicketMessage. Discarding the message ...");
                        return true;
                    }
                    logger.log(Severity.info, "Start processing ticketMessage ...");

                    var fields = ticketMessage.TextData.SplitCSV();
                    IRBCTicketCreator ticketCreator = RBCTicketCreatorFactory.GetTicketCreator(fields, true);
                    var creator = ticketCreator as CSxCAMergerLikeCreator;
                    if (creator != null)
                    {
                        bool reject = creator.GetTicketMessage(ref ticketMessage, fields);
                        string msg = !reject
                                    ? "TicketMessage processed!"
                                    : "TicketMessage has been rejected! Will not be processed by IS";
                        logger.log(Severity.info, msg);
                        return reject;
                    }
                    else
                    {
                        logger.log(Severity.info, "Ticket is not a valid CA type. Discarding ...");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    logger.log(Severity.error, "Exception is thrown: " + e.Message);
                    logger.log(Severity.debug, "Stack trace : " + e.StackTrace);
                    return true;
                }
                
            }
        }

    }
}
