using System;
using System.Collections.Generic;
using System.Reflection;
using Mediolanum_RMA_FILTER.TicketCreator.AbstractBase;
using Mediolanum_RMA_FILTER.Tools;
using RichMarketAdapter.ticket;
using sophis.log;
using transaction;
using System.Globalization;

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxGenericTrade : BaseTicketCreator
    {
        private const string COMMENT_SEPA = "#*#";
        private static string _ClassName = typeof(CSxGenericTrade).Name;
        public CSxGenericTrade(eRBCTicketType type) : base(type) {}

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);

                string _tradeType = fields.GetValue(RBCTicketType.GenericTrade.TradeType);
                string _instrumentRef = fields.GetValue(RBCTicketType.GenericTrade.InstrumentRef);
                string _bookId = fields.GetValue(RBCTicketType.GenericTrade.BookId);
                string _externalRef = fields.GetValue(RBCTicketType.GenericTrade.ExternalRef);
                string _quantity = fields.GetValue(RBCTicketType.GenericTrade.Quantity);
                string _spot = fields.GetValue(RBCTicketType.GenericTrade.Spot);
                string _spotType = fields.GetValue(RBCTicketType.GenericTrade.SpotType);
                string _tradeDate = fields.GetValue(RBCTicketType.GenericTrade.TradeDate);
                string _valueDate = fields.GetValue(RBCTicketType.GenericTrade.ValueDate);
                string _counterpartyId = fields.GetValue(RBCTicketType.GenericTrade.CounterpartyId);
                string _depositaryId = fields.GetValue(RBCTicketType.GenericTrade.DepositaryId);
                string _brokerId = fields.GetValue(RBCTicketType.GenericTrade.BrokerId);
                string _entity = fields.GetValue(RBCTicketType.GenericTrade.Entity);
                string _eventId = fields.GetValue(RBCTicketType.GenericTrade.EventId);
                string _currency = fields.GetValue(RBCTicketType.GenericTrade.Currency);
                string _comments = fields.GetValue(RBCTicketType.GenericTrade.Comments);
                string _userId = fields.GetValue(RBCTicketType.GenericTrade.UserId);
                string _extraFields = fields.GetValue(RBCTicketType.GenericTrade.ExtraFields);

                logger.log(Severity.debug, $"SetTicketMessage.BEGIN(_tradeType={_tradeType}, _instrumentRef={_instrumentRef}, _bookId={_bookId}, _externalRef ={_externalRef}, _quantity={_quantity}, _spot={_spot}, _tradeDate={_tradeDate}, _valueDate={_valueDate}, _counterpartyId={_counterpartyId}, _depositaryId={_depositaryId}, _brokerId={_brokerId}, _entity={_entity}, _eventId={_eventId}, _currency={_currency}, _userId={_userId}, _extraFields={_extraFields})");

                ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, _BusinessEvents[_tradeType]);
                ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, _instrumentRef);
                ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, _bookId);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, _externalRef);
                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, _quantity);
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, _spot);
                ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, _spotType);
                if (!string.IsNullOrEmpty(_tradeDate))
                {
                    try
                    {
                        int tradeDate = DateTime.ParseExact(_tradeDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToSophisDate();
                        ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                    }
                    catch (Exception e)
                    {
                        logger.log(Severity.error, e);
                    }
                }
                if (!string.IsNullOrEmpty(_valueDate))
                {
                    try
                    {
                        int settlDate = DateTime.ParseExact(_valueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToSophisDate();
                        ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlDate);
                        ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlDate);
                    }
                    catch (Exception e)
                    {
                        logger.log(Severity.error, e);
                    }
                }
                ticketMessage.SetTicketField(FieldId.COUNTERPARTYID_PROPERTY_NAME, _counterpartyId);
                ticketMessage.SetTicketField(FieldId.DEPOSITARYID_PROPERTY_NAME, _depositaryId);
                ticketMessage.SetTicketField(FieldId.BROKERID_PROPERTY_NAME, _brokerId);
                ticketMessage.SetTicketField(FieldId.ENTITY_PROPERTY_NAME, _entity);
                ticketMessage.SetTicketField(FieldId.CREATION_UPDATE_EVENT_ID, _eventId);
                ticketMessage.SetTicketField(FieldId.FX_CURRENCY_NAME, _currency);
                if (!string.IsNullOrEmpty(_extraFields)) _comments = $"{_comments}{COMMENT_SEPA}{_extraFields}";
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, _comments);
                ticketMessage.SetTicketField(FieldId.USERID_PROPERTY_NAME, _userId);

                logger.log(Severity.debug, "SetTicketMessage.END");
                return false;
            }
        }
    }
}
