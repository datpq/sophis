using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Mediolanum_RMA_FILTER.TicketCreator.AbstractBase;
using Mediolanum_RMA_FILTER.Tools;
using MEDIO.CORE.Tools;
using RichMarketAdapter.ticket;
using sophis.log;
using transaction;

using Oracle.DataAccess.Client;
using MEDIO.CORE.Tools;
using SophisETL.ISEngine;


namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxTermDepositCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxTermDepositCreator).Name;

        public CSxTermDepositCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(GetType(), MethodBase.GetCurrentMethod().Name))
            {
                //Check if instrument exists, if not create the message...
                bool instrumentInDB = false;
                string brokerBicCode = fields.GetValue(RBCTicketType.TermDepositColumns.BrokerBICCode);
                string startDate = fields.GetValue(RBCTicketType.TermDepositColumns.SettlementDate);
                string interestRate = Convert.ToDouble(fields.GetValue(RBCTicketType.TermDepositColumns.InterestRate)).ToString("0.0000");
                string maturityDate = fields.GetValue(RBCTicketType.TermDepositColumns.MaturityDate);
                string ccy = fields.GetValue(RBCTicketType.TermDepositColumns.Currency);
                string newStartDate = startDate.Replace("/", "");
                string newMaturityDate = maturityDate.Replace("/", "");
                string reference = brokerBicCode.Substring(0, 2) + newStartDate.Substring(0, 4) + newStartDate.Substring(6) + " " + interestRate + " " + newMaturityDate.Substring(0, 4) + newMaturityDate.Substring(6);
                string name = reference + " " + ccy;

                instrumentInDB = CheckInstrumentInDB(reference);

                if (!instrumentInDB)
                {
                    logger.log(Severity.debug, "Instrument with reference : " + reference + " not found in database, creating it via IS call.");
                    bool instrumentcreated = CreateInstrumentWithIS(reference, ccy, startDate, maturityDate, interestRate);
                    if (!instrumentcreated)
                        return true;

                }

                base.SetTicketMessage(ref ticketMessage, fields);
                string ExtFundId = fields.GetValue(RBCTicketType.TermDepositColumns.ExternalFundIdentifier);
                if (!CheckAllowedListExtFundId(ExtFundId))
                {
                    logger.log(Severity.warning, "Ignoring the ticket because ExtFundIdFilterFile parameter is enabled and [ " + ExtFundId + " ] is not part of allowed external fund identifier list.");
                    return true;
                }
                ticketMessage.SetUserField(RBCCustomParameters.Instance.RBCTransactionIDName, fields.GetValue(RBCTicketType.TermDepositColumns.TransactionID));
                ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
                string reversalStr = fields.GetValue(RBCTicketType.TermDepositColumns.ReversalFlag);
                bool reversalFlag = reversalStr.ToUpper().Equals("Y");
                SetDepositary(ref ticketMessage, ExtFundId);
                //SetBrokerID(ref ticketMessage, ExtFundId);

                // Only for term deposits adjust to CASH folder :
                //setfolioID returns the selected book id
                logger.log(Severity.debug, "Term Deposit Instrument, adjusting folio to CASH Folio");
                int bookId = SetFolioID(ref ticketMessage, ExtFundId); //setfolioID returns the bookId
                int cashBookId = bookId;
                string sql = "Select IDENT FROM (SELECT IDENT,NAME FROM FOLIO START WITH IDENT IN (:ident) CONNECT BY MGR = PRIOR IDENT) WHERE UPPER(NAME)='CASH'";
                OracleParameter param0 = new OracleParameter("ident", bookId);
                List<OracleParameter> parameters = new List<OracleParameter>() { param0 };
                cashBookId = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                logger.log(Severity.debug, "Found Cash book ident = " + cashBookId + " for folio ident = " + bookId);
                ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, cashBookId);

               //SetBrokerID(ref ticketMessage, ExtFundId);
                int ctpyID = SetCounterpartyID(ref ticketMessage, ExtFundId, _RbcTicketType, brokerBicCode);
                ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, GetEntityNameByID(ctpyID));
                ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, 1);
                ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, fields.GetValue(RBCTicketType.TermDepositColumns.DepositAmount));

                ticketMessage.SetTicketField(FieldId.FX_CURRENCY_NAME, ccy);
                int tradeDate = fields.GetValue(RBCTicketType.TermDepositColumns.TradeDate).GetDateInAnyFormat(CSxRBCHelper.CommonDateFormats);
                int settlDate = fields.GetValue(RBCTicketType.TermDepositColumns.SettlementDate).GetDateInAnyFormat(CSxRBCHelper.CommonDateFormats);
                ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, settlDate);
                ticketMessage.SetTicketField(FieldId.VALUEDATE_PROPERTY_NAME, settlDate);
                // string startDate = CSxUtils.GetDateFromSophisTime(tradeDate).ToString("yyyyMMdd");
                // int iMaturityDate = fields.GetValue(RBCTicketType.TermDepositColumns.MaturityDate).GetDateInAnyFormat(CSxRBCHelper.CommonDateFormats);
                // string endDate = CSxUtils.GetDateFromSophisTime(iMaturityDate).ToString("yyyyMMdd");

                // string rate = Convert.ToDouble(fields.GetValue(RBCTicketType.TermDepositColumns.InterestRate)).ToString("0.0000");
                //string instrumentName = RBCCustomParameters.Instance.TermDepositInstrumentNameFormat;
                //instrumentName = instrumentName.Replace("%StartDate%", startDate);
                //instrumentName = instrumentName.Replace("%Rate%", rate);
                //instrumentName = instrumentName.Replace("%EndDate%", endDate);
                int sicovam = 0;
                GetSicovamByName(name, out sicovam);
                ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, sicovam);
                SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                //if MAML Creation Event ID : ignore log + return true;
                // Only for term deposits:
                if (ticketMessage.getLong(FieldId.CREATION_UPDATE_EVENT_ID) == RBCCustomParameters.Instance.MAMLTradeCreationEventId)
                {
                    logger.log(Severity.warning, "Ignoring the tickest with MAML Creation Event");
                    return true;
                }
                string generatedHash = GenerateSha1Hash(fields, RBCTicketType.TermDepositColumns.ReversalFlag);
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, ((reversalFlag) ? ("R") : ("")) + generatedHash);
            }
            return false;
        }

        private bool CheckInstrumentInDB(string reference)
        {
            bool retval = false;

            using (Logger logger = new Logger(GetType(), MethodBase.GetCurrentMethod().Name))
            {


                try
                {
                    int count = 0;
                    string sql = "select count(*) from TITRES where REFERENCE = :ref";
                    OracleParameter param0 = new OracleParameter(":ref", reference);
                    List<OracleParameter> parameters = new List<OracleParameter>() { param0 };
                    count = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));

                    if (count == 1)
                        retval = true;
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Exception Occured : "+ex.Message);
                }
            }
            return retval;
        }

        bool CreateInstrumentWithIS(string reference, string ccy, string startDate, string maturityDate, string interestRate)
        {
            bool retval = false;

            using (Logger logger = new Logger(GetType(), MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    IntegrationServiceEngine _ISinstance = IntegrationServiceEngine.Instance;

                    string message = GetTermDepositMessage(reference, ccy, startDate, maturityDate, interestRate);
                    XDocument response = _ISinstance.Import(message);
                    string sResponse = response.ToString();

                    if (sResponse.Contains("Accepted"))
                    {
                        logger.log(Severity.debug, "Import Message Accepted.");
                        retval = true;
                    }
                    else
                    {
                        logger.log(Severity.error, "Import Message Rejected, Check IS logs");
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Exception Occured : " + ex.Message);
                }
            }

            return retval;


        }

        string GetTermDepositMessage(string reference, string ccy, string startDate, string maturityDate, string interestRate)
        {
            string retval = "";
            using (Logger logger = new Logger(GetType(), MethodBase.GetCurrentMethod().Name))
            {
                double rate = Convert.ToDouble(interestRate) / 100;


                 retval = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>"
                               + "<exch:import version=\"4-2\" xmlns:exch=\"http://sophis.net/sophis/gxml/dataExchange\">"
                               + "<sph:debtInstrument xmlns:sph=\"http://www.sophis.net/Instrument\" xmlns:common=\"http://sophis.net/sophis/common\">"
                               + "<sph:productType>TREAS-Debt Instrument-All|TREAS-Debt Instrument-Fixed Rate</sph:productType>"
                               + "<sph:feature>Without Market</sph:feature>"
                               + "<sph:identifier><sph:reference sph:modifiable=\"UniqueNotPrioritary\" sph:name=\"Sophisref\">" + reference + "</sph:reference></sph:identifier>"
                               + "<sph:name>" + reference + " " + ccy + "</sph:name>"
                               + "<sph:currency>" + ccy + "</sph:currency>"
                               + "<sph:pointValue>"
                               + "<sph:quotationType>InPrice</sph:quotationType>"
                               + "<sph:inYield>"
                               + "<sph:adjustedDate>true</sph:adjustedDate>"
                               + "<sph:settlementDate>true</sph:settlementDate>"
                               + "<sph:dayCountBasis sph:version=\"ISDA06\">ACT/360</sph:dayCountBasis>"
                               + "<sph:yield>Linear</sph:yield>"
                               + "</sph:inYield>"
                               + "</sph:pointValue>"
                               + "<sph:allotment>TERM DEPOSIT</sph:allotment>"
                               + "<sph:status>Available</sph:status>"
                               + "<sph:notional><sph:currency>" + ccy + "</sph:currency>"
                               + "<sph:amount>1.000000000000</sph:amount>"
                               + "</sph:notional>"
                               + "<sph:extraFields/>"
                               + "<sph:pricing><sph:nature>T</sph:nature>"
                               + "<sph:model>Standard</sph:model>"
                               + "<sph:spread>"
                               + "<sph:value>0.000000000000</sph:value>"
                               + "<sph:type>Undefined</sph:type>"
                               + "</sph:spread></sph:pricing>"
                               + "<sph:interestType>"
                               + "<sph:fixed>"
                               + "<sph:nominalRate>" + rate.ToString() + "</sph:nominalRate>"
                               + "<sph:dayCountBasis sph:version=\"ISDA06\">ACT/360</sph:dayCountBasis>"
                               + "<sph:interestCalculation>Linear</sph:interestCalculation>"
                               + "<sph:couponPaid>Afterward</sph:couponPaid>"
                               + "<sph:computationMethod>Afterward</sph:computationMethod>"
                               + "</sph:fixed>"
                               + "</sph:interestType>"
                               + "<sph:cashFlowGeneration>"
                               + "<sph:periodicityType>Final</sph:periodicityType>"
                               + "<sph:issueDate>" + startDate.Substring(6, 4) + "-" + startDate.Substring(3, 2) + "-" + startDate.Substring(0, 2) + "</sph:issueDate>"
                               + "<sph:redemptionDate>" + maturityDate.Substring(6, 4) + "-" + maturityDate.Substring(3, 2) + "-" + maturityDate.Substring(0, 2) + "</sph:redemptionDate>"
                               + "</sph:cashFlowGeneration>"
                               + "<sph:RedemptionDate>Included</sph:RedemptionDate>"
                               + "</sph:debtInstrument>"
                               + "</exch:import>";
                logger.log(Severity.debug, "Returning XML import message : " + retval);
            }
            return retval;
        }
    }
}
