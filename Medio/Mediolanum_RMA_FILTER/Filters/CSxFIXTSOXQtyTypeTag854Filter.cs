using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichMarketAdapter.interfaces;
using Mediolanum_RMA_FILTER.Tools;
using sophis.log;
using RichMarketAdapter.ticket;
using QuickFix;
using System.Reflection;
using transaction;
using Sophis.DataAccess;
using Oracle.DataAccess.Client;
using MEDIO.CORE.Tools;

namespace Mediolanum_RMA_FILTER.Filters
{
    class CSxFIXTSOXQtyTypeTag854Filter : IFilter
    {
        private const string STR_IS_UNIT_TRADED = "IS_UNIT_TRADED";
        private enum Tag
        {
            LastQty = 32,
            QtyType = 854
        }

        private enum QtyTypeTag854
        {
            UNITS = 0,
            CONTRACTS = 1
        }

        public class CSxFIXMessageHelper
        {
            public static string Extract(Message message, int tag)
            {
                if (message.IsSetField(tag))
                    return message.GetString(tag);
                if (message.Header.IsSetField(tag))
                    return message.Header.GetString(tag);
                if (message.Trailer.IsSetField(tag))
                    return message.Trailer.GetString(tag);
                return null;
            }
            public static int ExtractInt(Message message, int tag)
            {
                if (message.IsSetField(tag))
                    return message.GetInt(tag);
                if (message.Header.IsSetField(tag))
                    return message.Header.GetInt(tag);
                if (message.Trailer.IsSetField(tag))
                    return message.Trailer.GetInt(tag);
                return 0;
            }
            public static double ExtractDouble(Message message, int tag)
            {
                if (message.IsSetField(tag))
                    return decimal.ToDouble(message.GetDecimal(tag));
                if (message.Header.IsSetField(tag))
                    return decimal.ToDouble(message.Header.GetDecimal(tag));
                if (message.Trailer.IsSetField(tag))
                    return decimal.ToDouble(message.Trailer.GetDecimal(tag));
                return 0.0;
            }
        }

        private static string _ClassName = typeof(CSxFIXTSOXQtyTypeTag854Filter).Name;

        public bool filter(IMessageWrapper message)
        {
            bool status = false;
            using (var logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    if (message == null)
                    {
                        logger.log(Severity.warning, "Received message is null.");
                        return status;
                    }
                    ITicketMessage ticketMessage = (ITicketMessage)message.Message;
                    if (ticketMessage == null)
                    {
                        logger.log(Severity.warning, "ticketMessage cannot be cast as ITicketMessage.");
                        return status;
                    }
                    var quickFixMessage = (Message)ticketMessage.TransientData;
                    if (quickFixMessage == null)
                    {
                        logger.log(Severity.warning, "TransientData cannot be cast as QuickFixMessage.");
                        return status;
                    }

                    logger.log(Severity.info, "Start processing ticketMessage ...");
                    CheckQuantityTypeConsistency(quickFixMessage, ticketMessage);
                    UpdateTicketMessage(quickFixMessage, ticketMessage);
                    logger.log(Severity.info, "End processing ticketMessage ...");
                }
                catch (Exception e)
                {
                    logger.log(Severity.error, "Exception is thrown: " + e.ToString());
                    logger.log(Severity.debug, "Stack trace : " + e.StackTrace);
                    ITicketMessage ticketMessage = (ITicketMessage)message.Message;
                    if (ticketMessage != null)
                    {
                        logger.log(Severity.error, "Setting QUANTITY to ZERO to STOP the EXECUTION  because of tag 854 / IS_UNIT_TARDED incompatibility.");
                        double qtyZero = 0.0;
                        ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, qtyZero);
                    }
                    throw;
                }
            }
            return status;
        }

        private void UpdateTicketMessage(Message quickFixMessage, ITicketMessage ticketMessage)
        {
            using (var logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.info, "Updating Ticket Message...");
                int qtyType = CSxFIXMessageHelper.ExtractInt(quickFixMessage, (int)Tag.QtyType);
                double lastQty = CSxFIXMessageHelper.ExtractDouble(quickFixMessage, (int)Tag.LastQty);

                // ATTEMPT TO CHANGE THE ACCRUED AMOUNT HERE...BASED ON SELL AND IF AMOUNT < 0;
                if (quickFixMessage.IsSetField(54))
                {
                    int direction = quickFixMessage.GetInt(54);
                    logger.log(Severity.debug, "Field 54 found, value = " + direction);

                    if (direction == 2)
                    {
                        logger.log(Severity.debug, "Set lastqty to its opposite ");
                        lastQty = -lastQty;
                    }
                    else
                    {
                        logger.log(Severity.debug, "Not a sell. Do nothing.");
                    }
                }

                logger.log(Severity.info, String.Format("Extracted from QuickFixMessage - QtyType:{0}, LastQty:{1}", qtyType, lastQty));
                if (qtyType == (int)QtyTypeTag854.UNITS)
                {
                    logger.log(Severity.info, String.Format("Handling QtyType:{0}. Setting Sophis TicketField:{1} to {2}", QtyTypeTag854.UNITS.ToString(), FieldId.NOTIONAL_PROPERTY_NAME.ToString(), lastQty));
                    ticketMessage.SetTicketField(FieldId.NOTIONAL_PROPERTY_NAME, lastQty);
                }
                else if (qtyType == (int)QtyTypeTag854.CONTRACTS)
                {
                    logger.log(Severity.info, String.Format("Handling QtyType:{0}. Setting Sophis TicketField:{1} to {2}", QtyTypeTag854.CONTRACTS.ToString(), FieldId.QUANTITY_PROPERTY_NAME.ToString(), lastQty));
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, lastQty);
                }


                else
                {
                    logger.log(Severity.debug, "Field 54 not found");
                }
            }
        }

        private void CheckQuantityTypeConsistency(Message quickFixMessage, ITicketMessage ticketMessage)
        {
            using (var logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.info, "Checking Quantity Type Consistency...");
                // Check instrument reference
                if (!ticketMessage.contains(FieldId.MA_INSTRUMENT_NAME))
                {
                    string msg = "Instrument Reference [MA_INSTRUMENT_NAME] is not found in the ticketMessage.";
                    logger.log(Severity.error, msg);
                    throw new Exception(msg);
                }
                string instrRef = ticketMessage.getString(FieldId.MA_INSTRUMENT_NAME);
                if (string.IsNullOrEmpty(instrRef))
                {
                    string msg = "Instrument Reference is not defined.";
                    logger.log(Severity.error, msg);
                    throw new Exception(msg);
                }
                bool isUnitTraded = CheckIfBondIsUnitTraded(instrRef);
                int qtyType = CSxFIXMessageHelper.ExtractInt(quickFixMessage, (int)Tag.QtyType);
                string errorMsg = "";
                if (qtyType == (int)QtyTypeTag854.CONTRACTS)
                {
                    if (!isUnitTraded)
                        errorMsg = String.Format("ConsistencyCheckFailed: FIX Tag 854 (QtyType) is 1, but IS_UNIT_TRADED on the instrument {0} is either N or is not defined.", instrRef);
                }
                else if (qtyType == (int)QtyTypeTag854.UNITS)
                {
                    if (isUnitTraded)
                        errorMsg = String.Format("ConsistencyCheckFailed: FIX Tag 854 (QtyType) is 0,  but IS_UNIT_TRADED on the instrument {0} is Y.", instrRef);
                }
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    logger.log(Severity.error, errorMsg);
                    throw new Exception(errorMsg);
                }
            }
        }

        private static void InitializeDBConnection()
        {

            if (DBContext.Connection != null)
                return;
            using (var logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    logger.log(Severity.info, "Initializing DBConnection...");
                    // create a new connection  
                    string connectionString = sophis.configuration.CommonConfigurationGroup.Current.RisqueDatabaseSection.ConnectionString;

                    OracleConnection myConnection = new OracleConnection(connectionString);
                    // this retrieves the 1st private static field of type OracleConnection  
                    // currently there's only one of such fields  
                    FieldInfo sharedConnectionField = typeof(Sophis.DataAccess.DBContext).GetFields(BindingFlags.NonPublic | BindingFlags.Static).FirstOrDefault(f => f.FieldType == typeof(OracleConnection));
                    // set the value  
                    // 1st argument is NULL because this is a static field  
                    sharedConnectionField.SetValue(null, myConnection);
                    // open the connection  
                    DBContext.Connection.Open();
                    logger.log(Severity.info, "Opened DBConnection...");
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, String.Format("Failed to Initialize DBConnection! {0}.", ex.ToString()));
                    throw;
                }
            }
        }

        private static string GetQueryToCheckIfBondIsUnitTraded(string instrRef, bool selectCount = false)
        {
            string query = String.Format(@"SELECT
                                                {0}
                                            FROM
                                                sector_instrument_association   a,
                                                sectors                         p,
                                                sectors                         r,
                                                titres                          t
                                            WHERE
                                                a.sicovam = t.sicovam
                                                AND a.type = p.id
                                                AND a.sector = r.id
                                                AND p.name = '{1}'
                                                AND t.reference = '{2}'",
                                                                        (selectCount ? " count(*) " : " r.name "),
                                                      STR_IS_UNIT_TRADED,
                                                      instrRef
                                                      );
            return query;
        }
        public static bool CheckIfBondIsUnitTraded(string instrRef)
        {
            bool isUnitTraded = false;
            using (var logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.info, String.Format("Checking if the bond {0} IS_UNIT_TRADED...", instrRef));
                // Workaround to avoid exception if the property is not defined.
                string queryGetRowCount = GetQueryToCheckIfBondIsUnitTraded(instrRef, true);
                InitializeDBConnection();
                int rowCount = Convert.ToInt32(CSxDBHelper.GetOneRecord(queryGetRowCount));
                logger.log(Severity.info, String.Format("IS_UNIT_TRADED on Bond {0} is {1} defined: ", instrRef, ((rowCount != 1) ? "not" : "")));
                if (rowCount == 1)  // Property is defined. Let's check if it is YES or NO.
                {
                    string query = GetQueryToCheckIfBondIsUnitTraded(instrRef);
                    logger.log(Severity.info, String.Format("Getting IS_UNIT_TRADED value of the bond {0} using the query {1}", instrRef, query));
                    isUnitTraded = CSxDBHelper.GetOneRecord<string>(query).ToUpper().Equals("Y");
                }
            }
            return isUnitTraded;
        }
    }
}
