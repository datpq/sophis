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
using QuickFix;

namespace Mediolanum_RMA_FILTER.Filters
{
    public class CSxEMSXFilter : IFilter
    {
        private static string _ClassName = typeof(CSxRBCCAMergerFilter).Name;

        public CSxEMSXFilter()
            : base()
        {
        }

        public bool filter(IMessageWrapper message)
        {
            using (var logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    if (message == null)
                    {
                        logger.log(Severity.warning, "Received message is null.");
                        return false;
                    }
                    ITicketMessage ticketMessage = (ITicketMessage)message.Message;
                    if (ticketMessage == null)
                    {
                        logger.log(Severity.warning, "ticketMessage cannot be cast as ITicketMessage.");
                        return false;
                    }

                    if (ticketMessage.contains(FieldId.MA_INSTRUMENT_NAME))
                    {
                        string instRef = ticketMessage.getString(FieldId.MA_INSTRUMENT_NAME);
                        logger.log(Severity.debug, "instrument ref = " + instRef);
                        if( instRef.Contains("Curncy")  == true )
                        {
                            logger.log(Severity.debug, "instrument ref contains Curncy. Change the spot type.");
                            if(ticketMessage.contains(FieldId.SPOTTYPE_PROPERTY_NAME) == true)
                            {
                                logger.log(Severity.debug, "ticket message contains SpotType. Remove it.");
                                ticketMessage.remove(FieldId.SPOTTYPE_PROPERTY_NAME);
                            }
                            logger.log(Severity.debug, "Force SpotType to InPrice");
                            ticketMessage.add(FieldId.SPOTTYPE_PROPERTY_NAME, transaction.SpotTypeConstants.IN_PRICE);
                        }
                    }
                    else
                    {
                        logger.log(Severity.warning, "ticketMessage does not contain a INSTRUMENTREF field.");
                    }
                }
                catch (Exception e)
                {
                    logger.log(Severity.error, "Exception is thrown: " + e.Message);
                    logger.log(Severity.debug, "Stack trace : " + e.StackTrace);
                }
                return false;

            }
        }

    }
}
