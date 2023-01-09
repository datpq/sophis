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
    public class CSxFIXTSOXTag54 : IFilter
    {
        private static string _ClassName = typeof(CSxFIXTSOXTag54).Name;

        public CSxFIXTSOXTag54()
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
                    if (quickFixMessage.IsSetField(54))
                    {
                        int direction = quickFixMessage.GetInt(54);
                        logger.log(Severity.debug, "Field 54 found, value = " + direction);
                        if (direction == 2)
                        {
                            logger.log(Severity.debug, "Set ACCRUEDAMOUNT_PROPERTY_NAME to its opposite ");

                            double accrued = -1 * ticketMessage.getDouble(FieldId.ACCRUED_PROPERTY_NAME) ?? 0;
                          
                            if (accrued != 0)
                            {
                                logger.log(Severity.debug, "Opposite retrievied : " + accrued);
                                ticketMessage.SetTicketField(FieldId.ACCRUEDAMOUNT_PROPERTY_NAME, accrued);
                            }
                            else
                            {
                                logger.log(Severity.debug, "Null Value returned for accrued from ticket message. Do nothing. ");
                            }
                        }
                        else
                        {
                            logger.log(Severity.debug, "Not a sell. Do nothing.");
                        }
                    }
                    else
                    {
                        logger.log(Severity.debug, "Field 54 not found");
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
