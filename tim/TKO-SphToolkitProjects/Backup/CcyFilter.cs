using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichMarketAdapter.interfaces;
using RichMarketAdapter.ticket;
using sophis.log;
using transaction;
using sophis.wcfFramework.config;
using QuickFix;

namespace TKO_OmsPenceAdapter
{
    public class CcyFilter : IFilter
    {
        public enum Tag
        {
            AveragePrice = 6,
            FixTypeTag = 8,
            OrderSet = 11,
            Currency = 15,
            TradeExecID = 17,
            LastQuantity = 32,
            OrderRef = 37,
            SecurityID = 48,
            SendingDate = 52,
            Side = 54, // 1 = Buy / 2 = Sell
            Operator = 57,
            MarketWay = 55,
            ExecBroker = 76,
            SettlementCcy = 120,
            OrderStatus = 150,
            SecurityType = 167,
        }

        public string Extract(Message message, int tag)
        {
            if (message.isSetField(tag))
                return message.getString(tag);
            if (message.getHeader().isSetField(tag))
                return message.getHeader().getString(tag);
            if (message.getTrailer().isSetField(tag))
                return message.getTrailer().getString(tag);
            return null;
        }
        public bool filter(IMessageWrapper message)
        {
            using (var log = new Logger(this, "filter"))
            {
                var ticket = (ITicketMessage)message.Message;
                var quickFixMessage = (Message)ticket.TransientData;

                var oldCurrency = Extract(quickFixMessage, (int)Tag.Currency);
                log.log(Severity.debug, String.Format("[TKT] Old currency is-{0}", oldCurrency));
                if (oldCurrency == "GBp")
                {
                    string newCurrency = oldCurrency.ToUpperInvariant();
                    log.log(Severity.debug, String.Format("[TKT] Modifying currency to[ {0} ]", newCurrency));
                    newCurrency = "GBP";

                    UpdateMessage(message, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, newCurrency);
                    UpdateMessage(message, FieldId.FX_CURRENCY_NAME, newCurrency);

                    string ticketPaymentCCy = ((ITicketMessage)message.Message).getString(FieldId.PAYMENTCURRENCY_PROPERTY_NAME);
                    string ticketFxCurrencyName = ((ITicketMessage)message.Message).getString(FieldId.FX_CURRENCY_NAME);

                    log.log(Severity.debug, String.Format("[TKT] Display Ticket to[ {0} ]", ticketPaymentCCy));
                    log.log(Severity.debug, String.Format("[TKT] Display Ticket to[ {0} ]", ticketFxCurrencyName));
                }
                if (oldCurrency == "USd")
                {
                    string newCurrency = oldCurrency.ToUpperInvariant();
                    log.log(Severity.debug, String.Format("[TKT] Modifying currency to[ {0} ]", newCurrency));
                    newCurrency = "USD";

                    UpdateMessage(message, FieldId.PAYMENTCURRENCY_PROPERTY_NAME, newCurrency);
                    UpdateMessage(message, FieldId.FX_CURRENCY_NAME, newCurrency);

                    string ticketPaymentCCy = ((ITicketMessage)message.Message).getString(FieldId.PAYMENTCURRENCY_PROPERTY_NAME);
                    string ticketFxCurrencyName = ((ITicketMessage)message.Message).getString(FieldId.FX_CURRENCY_NAME);
                    
                    log.log(Severity.debug, String.Format("[TKT] Display Ticket to[ {0} ]", ticketPaymentCCy));
                    log.log(Severity.debug, String.Format("[TKT] Display Ticket to[ {0} ]", ticketFxCurrencyName));
                }

                return false;
            }
        }


        private void UpdateMessage<T>(IMessageWrapper message, FieldId fieldId, T value)
        {
            using (var log = new Logger(this, "filter"))
            {
                log.log(Severity.debug, "<<< Update Message");
                if (value is string)
                {
                    log.log(Severity.debug, String.Format("[TKT] UpdateMessage String[ {0} ]", Convert.ChangeType(value, typeof(string))));

                    ((ITicketMessage)message.Message).add(fieldId, value);

                    log.log(Severity.debug, String.Format("[TKT] RMA MSG[ {0} ]", message.Message.TextData));
                }
                else if (value is Int32)
                {
                    log.log(Severity.debug, String.Format("[TKT] UpdateMessage INT32[ {0} ]", value));

                    ((ITicketMessage)message.Message).add(fieldId, Convert.ChangeType(value, typeof(int)));

                    log.log(Severity.debug, String.Format("[TKT] RMA MSG[ {0} ]", message.Message.TextData));
                }
                else if (value is double)
                {
                    log.log(Severity.debug, String.Format("[TKT] UpdateMessage Double-{0}", value));

                    ((ITicketMessage)message.Message).add(fieldId, Convert.ChangeType(value, typeof(double)));

                    log.log(Severity.debug, String.Format("[TKT] RMA MSG[ {0} ]", message.Message.TextData));
                }
                else if (value is DateTime)
                {
                    throw new NotImplementedException("DateTime is supported but needs to be checked and casted. Please update the toolkit to add this functionality.");
                }
                else
                {
                    throw new NotImplementedException("value type is neither string, int32, double or dateTime");
                }
                log.log(Severity.debug, ">>> Update Message");
            }
        }
    }

}
