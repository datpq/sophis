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
    public class CSxFIXTSOXDirtyPriceFilter : IFilter
    {
        private static string _ClassName = typeof(CSxRBCCAMergerFilter).Name;

        public CSxFIXTSOXDirtyPriceFilter()
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
                    var quickFixMessage = (Message)ticketMessage.TransientData;
                    if (quickFixMessage == null)
                    {
                        logger.log(Severity.warning, "TransientData cannot be cast as QuickFixMessage.");
                        return false;
                    }
                   
                    logger.log(Severity.info, "Start processing ticketMessage ...");
                    if (quickFixMessage.IsSetField(22251))
                    {
                        int priceQualifier = quickFixMessage.GetInt(22251);
                        logger.log(Severity.debug, "Field 22251 found, value = " + priceQualifier);
                        if (priceQualifier == 1 )
                        {
                            logger.log(Severity.debug, "Set ACCRUEDAMOUNT_PROPERTY_NAME to 0 ");
                            ticketMessage.SetTicketField(FieldId.ACCRUEDAMOUNT_PROPERTY_NAME, 0.0);
                        }
                        else
                        {
                            logger.log(Severity.debug, "22251 <> 1. Do nothing.");
                        }
                    }
                    else
                    {
                        logger.log(Severity.debug, "Field 22251 not found");
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
