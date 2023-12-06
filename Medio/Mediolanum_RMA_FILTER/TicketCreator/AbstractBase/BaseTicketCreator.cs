using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Mediolanum_RMA_FILTER.Data;
using Mediolanum_RMA_FILTER.Interfaces;
using Mediolanum_RMA_FILTER.Tools;
using MEDIO.CORE.Tools;
using Oracle.DataAccess.Client;
using RichMarketAdapter.ticket;
using sophis.log;
using sophis.portfolio;
using sophis.utils;
using sophis.value;
using transaction;
using System.Text.RegularExpressions;

namespace Mediolanum_RMA_FILTER.TicketCreator.AbstractBase
{
    public abstract class BaseTicketCreator : IRBCTicketCreator
    {
        #region Fields
        private static string _ClassName = typeof(BaseTicketCreator).Name;
        protected static Dictionary<string, int> _Rootportfolios = new Dictionary<string, int>();
        protected static List<Tuple<int /*accountid*/, int/*entityid*/, string /*account_at_custodian*/, string /*account_name*/>> _Delegatemanagers = new List<Tuple<int, int, string,string>>();
        protected static Dictionary<string, int> _BusinessEvents = new Dictionary<string, int>();
        protected eRBCTicketType _RbcTicketType;
        protected static List<string> _MAMLZCodesList = new List<string>();
        protected int DefaultSpotType = SpotTypeConstants.IN_PRICE;
        protected int BondSpotType = SpotTypeConstants.IN_PERCENTAGE;
        protected int FutureSFESpotType = SpotTypeConstants.IN_RATE;
        #endregion
        
        public BaseTicketCreator(eRBCTicketType type)
        {
            _Rootportfolios =   CSxCachingDataManager.Instance.GetItem(eMedioCachedData.Rootportfolios.ToString()) as Dictionary<string, int>;
            _BusinessEvents = CSxCachingDataManager.Instance.GetItem(eMedioCachedData.Businessevents.ToString()) as Dictionary<string, int>;
            _Delegatemanagers = CSxCachingDataManager.Instance.GetItem(eMedioCachedData.Delegatemanagers.ToString()) as List<Tuple<int, int, string,string>>;
            _RbcTicketType = type;
            _MAMLZCodesList = RBCCustomParameters.Instance.MAMLZCodesList;
        }

        #region IRBCTicket interface
        public virtual bool GetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            bool res = SetTicketMessage(ref ticketMessage, fields);
            ValidateTicketFields(ref ticketMessage, fields);
            return res;
        }

        public virtual void ValidateTicketFields(ref ITicketMessage ticketMessage, List<string> fields)
        {
        }

        public virtual bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            ticketMessage.remove(FieldId.QUANTITY_PROPERTY_NAME); //Quantity
            ticketMessage.remove(FieldId.SPOT_PROPERTY_NAME); // Price
            ticketMessage.remove(FieldId.INSTRUMENT_SOURCE); //UNIVERSAL 'external' mapping value
            ticketMessage.remove(FieldId.MA_COMPLEX_REFERENCE_TYPE); //UNIVERSAL (ISIN, BLOOMBERG, etc.)
            ticketMessage.remove(FieldId.INSTRUMENTTYPE_PROPERTY_NAME); //Bond, Equity, Forex, etc.
            ticketMessage.remove(FieldId.MA_INSTRUMENT_NAME); //ISIN Code or BLOOMBERG Code (depends on MA_COMPLEX_REFERENCE_TYPE)
            ticketMessage.remove(FieldId.NEGOTIATIONDATE_PROPERTY_NAME); //Trade date
            return false;
        }

        #endregion

        #region Functions
        protected bool CheckBBHFundId(string commonIdentifier)
        {
            try
            {
                return RBCCustomParameters.Instance.BBHFundIds.Contains(commonIdentifier.ToUpper());
            }
            catch
            {
                return false;
            }
        }

        protected bool CheckAllowedListExtFundId(string commonIdentifier)
        {
            bool retval = false;
            if (String.IsNullOrEmpty(RBCCustomParameters.Instance.ExtFundIdFilterFile))
            {
                retval = true; // allowed
            }
            else if (!String.IsNullOrEmpty(commonIdentifier))
            {
                if (RBCCustomParameters.Instance.AllowedExtFundIds.Contains(commonIdentifier.ToUpper()))
                {
                    retval = true;
                }
            }
            else
            {
                retval = true;
            }
            return retval;
        }

        protected int SetDepositary(ref ITicketMessage ticketMessage, string commonIdentifier)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int retval = 0;
                if (commonIdentifier.ValidateNotEmpty())
                {
                    int depo = GetDepositary(commonIdentifier);
                    logger.log(Severity.debug, String.Format("Got depositary from nostro account {0} = {1}", commonIdentifier, depo));
                    if (depo > 0)
                    {
                        ticketMessage.SetTicketField(FieldId.DEPOSITARYID_PROPERTY_NAME, depo);
                        retval = depo;
                    }
                    else
                    {
                        depo = GetPrimeBroker(commonIdentifier); // from root fund
                        logger.log(Severity.debug, String.Format("Got prime broker from fund {0} = {1}", commonIdentifier, depo));
                        if (depo > 0)
                        {
                            ticketMessage.SetTicketField(FieldId.DEPOSITARYID_PROPERTY_NAME, depo);
                            retval = depo;
                        }
                    }
                }
                return retval;
            }
        }

        public int GetDepositary(string commonIdentifier)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetDepositary start");
                int res = 0;
                if (!String.IsNullOrEmpty(commonIdentifier))
                {
                    try
                    {
                        logger.log(Severity.debug, "Trying to get depositary ID by commonIdentifier ...");
                        string sql = "select depositary from BO_TREASURY_ACCOUNT where ACCOUNT_AT_CUSTODIAN = :ACCOUNT_AT_CUSTODIAN";
                        OracleParameter parameter = new OracleParameter(":ACCOUNT_AT_CUSTODIAN", commonIdentifier);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                        res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred while trying to get depositary from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return res;
            }
        }

        protected int SetBrokerID(ref ITicketMessage ticketMessage, string commonIdentifier, string brokerBIC = null)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (!String.IsNullOrEmpty(commonIdentifier))
                {
                    string last8chars = commonIdentifier.GetLast8Characters();
                    if (!last8chars.ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "Checking if " + last8chars + " is a MAML Z Code");
                        if (_MAMLZCodesList.Contains(last8chars.ToUpper()) && brokerBIC.ValidateNotEmpty())
                        {
                            return 0;
                        }
                    }
                    logger.log(Severity.debug, "Getting broker by DelegeteManagerID ...");
                    // Use DelelgateManagerId
                    Tuple<int, int, string, string> delegatemanager = _Delegatemanagers.Find(x => (x.Item3.ToSafeString().ToUpper().CompareTo(commonIdentifier.ToUpper()) == 0));
                    if (delegatemanager != null)
                    {
                        logger.log(Severity.debug, "Found (delegate manager) broker id = " + delegatemanager.Item2 + " with account = " + commonIdentifier);
                        ticketMessage.SetTicketField(FieldId.BROKERID_PROPERTY_NAME, delegatemanager.Item2);
                        return delegatemanager.Item2;
                    }
                    else
                    {
                        logger.log(Severity.debug, "Could not find broker with account = " + commonIdentifier);
                        return 0;
                    }
                }
                else
                {
                    logger.log(Severity.debug, "Invalid argument, CommonIdentifier cannot be null or empty");
                    return 0;
                }
            }
        }

        protected int SetCounterpartyID(ref ITicketMessage ticketMessage, string commonIdentifier, eRBCTicketType type, string brokerBIC = null)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (commonIdentifier.ValidateNotEmpty())
                {
                    if (IsMAMLCommonIdentifeir(commonIdentifier) || (!IsMAMLCommonIdentifeir(commonIdentifier) && !RBCCustomParameters.Instance.UseDefaultCounterparty))
                    {
                        switch (type)
                        {
                            case eRBCTicketType.Cash:
                            case eRBCTicketType.TACash:
                            case eRBCTicketType.Invoice:
                            {
                                // use fund prime broker as counterparty
                                int primebrokerID = GetPrimeBroker(commonIdentifier);
                                if (primebrokerID != 0)
                                {
                                    ticketMessage.SetTicketField(FieldId.COUNTERPARTYID_PROPERTY_NAME, primebrokerID);
                                    return primebrokerID;
                                }
                                else
                                {
                                    logger.log(Severity.warning, "Failed to find a valid Prime Broker");
                                    return 0;
                                }
                            } break;
                            case eRBCTicketType.ForexHedge:
                            {
                                // use default hedge counterparty
                                if (RBCCustomParameters.Instance.DefaultFXHedgeCounterpartyId != 0)
                                {
                                    ticketMessage.SetTicketField(FieldId.COUNTERPARTYID_PROPERTY_NAME, RBCCustomParameters.Instance.DefaultFXHedgeCounterpartyId);
                                }
                                return 0;
                            } break;
                            default:
                            {
                                if (brokerBIC.ValidateNotEmpty())
                                {
                                    logger.log(Severity.debug, "Getting counterparty by BICCode (SWIFT)");
                                    int counterparty_id = GetEntityFromSWIFT(brokerBIC);
                                    if (counterparty_id != 0)
                                    {
                                        logger.log(Severity.debug, "Found counterparty id = " + counterparty_id + " with BICCode (SWIFT) = " + brokerBIC);
                                        ticketMessage.SetTicketField(FieldId.COUNTERPARTYID_PROPERTY_NAME, counterparty_id);
                                        return counterparty_id;
                                    }
                                    else
                                    {
                                        logger.log(Severity.debug, "Could not find counterparty with BICCode (SWIFT) = " + brokerBIC);
                                        return 0;
                                    }
                                }
                            } break;
                        }
                    }
                    else if (RBCCustomParameters.Instance.DefaultCounterpartyId != 0 && RBCCustomParameters.Instance.UseDefaultCounterparty)
                    {
                        ticketMessage.SetTicketField(FieldId.COUNTERPARTYID_PROPERTY_NAME, RBCCustomParameters.Instance.DefaultCounterpartyId);
                        return RBCCustomParameters.Instance.DefaultCounterpartyId;
                    }
                }
                else
                {
                    logger.log(Severity.error, "Invalid argument : CommonIdentifier cannot be null or empty");
                    return 0;
                }
                return 0;
            }
        }

        protected string GetEntityNameByID(int id)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetEntityNameByID start");
                string res = "";
                try
                {
                    string sql = "SELECT NAME FROM TIERS WHERE IDENT = :IDENT";
                    OracleParameter parameter = new OracleParameter(":IDENT", id);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                    res = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while trying to get entity from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                return res;
            }
        }

        protected int SetDefaultKernelWorkflow(ref ITicketMessage ticketMessage, string commonIdentifier)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int retval = 0;
                if (commonIdentifier.ValidateNotEmpty())
                {
                    if (RBCCustomParameters.Instance.OverrideCreationEvent)
                    {
                        logger.log(Severity.debug, "OverrideCreationEvent parameter enebled");
                        if (IsMAMLCommonIdentifeir(commonIdentifier) && RBCCustomParameters.Instance.MAMLTradeCreationEventId != 0)
                        {
                            logger.log(Severity.debug, "Overriding creation event to MAMLTradeCreationEvent");
                            ticketMessage.SetTicketField(FieldId.CREATION_UPDATE_EVENT_ID, RBCCustomParameters.Instance.MAMLTradeCreationEventId);
                            retval = RBCCustomParameters.Instance.MAMLTradeCreationEventId;
                        }
                        else if (RBCCustomParameters.Instance.DelegateTradeCreationEventId != 0)
                        {
                            logger.log(Severity.debug, "Overriding creation event to DelegateTradeCreationEvent");
                            ticketMessage.SetTicketField(FieldId.CREATION_UPDATE_EVENT_ID, RBCCustomParameters.Instance.DelegateTradeCreationEventId);
                            retval = RBCCustomParameters.Instance.DelegateTradeCreationEventId;
                        }
                    }
                }
                return retval;
            }
        }

        protected void SetDefaultFailureMessageFileds(ref ITicketMessage ticketMessage, bool doNotSetExtRef = false)
        {
            ticketMessage.SetTicketField(FieldId.MA_INSTRUMENTTYPE_PROPERTY_NAME, MAInstrumentTypeConstants.GENERAL);
            ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, RBCCustomParameters.Instance.DefaultErrorFolio);
            ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, 1);
            ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, RBCCustomParameters.Instance.DefaultErrorInstrumentID);
            ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, 0.0);
            ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, "Invalid");
            ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, "Invalid");
            if (RBCCustomParameters.Instance.OverwriteBORemarks && !doNotSetExtRef)
                ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, GenerateSha1Hash());
        }

        protected string GetInstrumentCCYByRef(string refName, string refValue)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                string res = "";
                try
                {
                    // first check
                    string sql = "select DEVISE_TO_STR(DEVISECTT) from titres where REFERENCE = :REFERENCE";
                    OracleParameter parameter = new OracleParameter(":REFERENCE", refValue);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                    res = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));

                    // second check
                    if (String.IsNullOrEmpty(res) && !String.IsNullOrEmpty(refName))
                    {
                        sql = "select DEVISE_TO_STR(t.DEVISECTT) from titres t"
                            + " inner join EXTRNL_REFERENCES_INSTRUMENTS ei"
                            + " on t.sicovam = ei.SOPHIS_IDENT"
                            + " inner join EXTRNL_REFERENCES_DEFINITION ed"
                            + " on ei.ref_ident = ed.ref_ident and ed.ref_name = :refName and ei.value = :refValue";

                        OracleParameter parameter1 = new OracleParameter(":refName", refName);
                        OracleParameter parameter2 = new OracleParameter(":refValue", refValue);
                        parameters = new List<OracleParameter>() { parameter1, parameter2};
                        res = Convert.ToString(CSxDBHelper.GetOneRecord(sql, parameters));
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred during GetUnderlyingCCY: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                logger.log(Severity.debug, "Underlying ccy found by instrument " + refValue + " = " + res);
                return res;
            }
        }

        protected int GetMarketByRef(string refName, string refValue)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int res = 0;
                try
                {
                    // first check
                    string sql = "select MARCHE from titres where REFERENCE = :REFERENCE";
                    OracleParameter parameter = new OracleParameter(":REFERENCE", refValue);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                    res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));

                    // second check
                    if (res <= 0 && !String.IsNullOrEmpty(refName))
                    {
                        sql = "select MARCHE from titres t"
                              + "inner join EXTRNL_REFERENCES_INSTRUMENTS ei"
                              + "on t.sicovam = ei.SOPHIS_IDENT"
                              + "inner join EXTRNL_REFERENCES_DEFINITION ed"
                              + "on ei.ref_ident = ed.ref_ident and ed.ref_name = :refName and ei.value = :refValue";

                        OracleParameter parameter1 = new OracleParameter(":refName", refName);
                        OracleParameter parameter2 = new OracleParameter(":refValue", refValue);
                        parameters = new List<OracleParameter>() { parameter1, parameter2 };
                        res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred during GetMarketByRef: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                logger.log(Severity.debug, "Market found by instrument " + refValue + " = " + res);
                return res;
            }
        }

        protected int SetFolioID(ref ITicketMessage ticketMessage, string commonIdentifier, string targetStrategy = null, string targetSubStrategy = null, bool forceStrategyFolio = false)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (commonIdentifier.ValidateNotEmpty())
                {
                    if (_Rootportfolios.Keys.Contains(commonIdentifier))
                    {
                        if ((RBCCustomParameters.Instance.UseStrategySubfolios || forceStrategyFolio) && !String.IsNullOrEmpty(targetStrategy))
                        {
                            int fundFolioID = _Rootportfolios[commonIdentifier];
                            int strategyFolioID = GetStrategyFolioID(fundFolioID, targetStrategy, targetSubStrategy);
                            if (strategyFolioID != 0)
                            {
                                logger.log(Severity.debug, "Selected (strategy) folio with id = " + strategyFolioID + " and name = " + targetStrategy + ((!String.IsNullOrEmpty(targetSubStrategy)) ? (" (" + targetSubStrategy + ")") : ("")));
                                ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, strategyFolioID);
                                return strategyFolioID;
                            }
                            else
                            {
                                logger.log(Severity.warning, "Could not find a valid strategy folio with name = " + targetStrategy + ((!String.IsNullOrEmpty(targetSubStrategy)) ? (" (" + targetSubStrategy + ")") : ("")) + ", selecting fund folio with ID = " + fundFolioID);
                                ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, fundFolioID);
                                return fundFolioID;
                            }
                        }
                        else
                        {
                            logger.log(Severity.warning, "Selected folio with id = " + _Rootportfolios[commonIdentifier]);
                            ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, _Rootportfolios[commonIdentifier]);
                            return _Rootportfolios[commonIdentifier];
                        }
                    }
                    else
                    {
                        logger.log(Severity.warning, "Could not find a valid portfolio associated with CommonIdentifier: " + commonIdentifier);
                    }
                }
                else
                {
                    logger.log(Severity.warning, "Invalid argument, CommonIdentifier cannot be null or empty");
                }
                return 0;
            }
        }

        protected int SetCashFolioID(ref ITicketMessage ticketMessage, string commonIdentifier)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "SetCashFolioID start");
                int res = 0;
                if (!commonIdentifier.ValidateNotEmpty())
                {
                    logger.log(Severity.warning, "Invalid argument, commonIdentifier cannot be null or empty");
                    return res;
                }
                logger.log(Severity.debug, "Trying to get Cash folio ID by ACCOUNT_LEVEL_FOLIO");
                string sql = "SELECT ACCOUNT_LEVEL_FOLIO FROM BO_TREASURY_ACCOUNT WHERE ID = (SELECT ID FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = :ACCOUNT_AT_CUSTODIAN)";
                OracleParameter parameter = new OracleParameter(":ACCOUNT_AT_CUSTODIAN", commonIdentifier);
                List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                if (res != 0)
                {
                    ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, res);
                    return res;
                }
                if (res == 0) // try again 
                {
                    sql = "SELECT VALUE FROM BO_TREASURY_EXT_REF WHERE REF_ID = (SELECT REF_ID FROM BO_TREASURY_EXT_REF_DEF WHERE REF_NAME = 'AlternativeCashFolioID') AND ACC_ID = (SELECT ID FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = :ACCOUNT_AT_CUSTODIAN)";
                    parameter = new OracleParameter(":ACCOUNT_AT_CUSTODIAN", commonIdentifier);
                    parameters = new List<OracleParameter>() { parameter };
                    res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    if (res != 0)
                    {
                        ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, res);
                        return res;
                    }
                }
                if (res == 0) // and again
                {
                    sql = "SELECT NAME,IDENT FROM FOLIO WHERE MGR = (SELECT VALUE FROM BO_TREASURY_EXT_REF WHERE REF_ID = (SELECT REF_ID FROM BO_TREASURY_EXT_REF_DEF WHERE REF_NAME = 'RootPortfolio')  AND ACC_ID = (SELECT ID FROM BO_TREASURY_ACCOUNT WHERE ACCOUNT_AT_CUSTODIAN = :ACCOUNT_AT_CUSTODIAN)";
                    parameter = new OracleParameter(":ACCOUNT_AT_CUSTODIAN", commonIdentifier);
                    parameters = new List<OracleParameter>() { parameter };
                    Dictionary<string, int> resDictionary = CSxDBHelper.GetDictionary<string, int>(sql, parameters);

                    foreach (var one in resDictionary)
                    {
                        if (one.Key.ToUpper().Contains("CASH"))
                        {
                            res = Convert.ToInt32(one.Value);
                            ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, res);
                            break;
                        }
                    }
                }
                if (res == 0) // we give up
                {
                    if (RBCCustomParameters.Instance.DefaultErrorFolio != 0)
                    {
                        res = RBCCustomParameters.Instance.DefaultErrorFolio;
                        ticketMessage.SetTicketField(FieldId.BOOKID_PROPERTY_NAME, res);
                        ticketMessage.SetTicketField(FieldId.COMMENTS_PROPERTY_NAME, "Unable to find folio for cash account");
                    }
                    else
                    {
                        logger.log(Severity.warning, "MediolanumRMA.DefaultErrorFolio custom parameter not set");
                    }
                }
                return res;
            }
        }

        protected bool IsFXPairReversed(int ccy1, int ccy2)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "IsFXPairReversed start");
                bool res = false;
                try
                {
                    string sql = "SELECT QUOTATION_TYPE FROM TITRES WHERE TYPE = 'E' AND MARCHE= :ccy1 AND DEVISECTT= :ccy2";
                    OracleParameter parameter = new OracleParameter(":ccy1", ccy1);
                    OracleParameter parameter1 = new OracleParameter(":ccy2", ccy2);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameter, parameter1};
                    int type = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    res = type == -1 ? true: false;
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred IsFXPairReversed: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                return res;
            }
        }

        protected int GetMasterCurrency(int ccy)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int res = 0;
                try
                {
                    string sql = "select MASTERCURRENCY from devisev2 where code = :ccy";
                    OracleParameter parameter = new OracleParameter(":ccy", ccy);
                    List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                    res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred GetMasterCurrency: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                return res;
            }
        }

        protected bool CheckAcceptedTransactionType(string type)
        {
            bool res = true;
            if (!String.IsNullOrEmpty(type))
            {
                if (RBCCustomParameters.Instance.UnacceptedTransactionTypeList.Contains(type.ToUpper()))
                    res = false; // not accepted
            }
            return res;
        }

        public int GetCashInstrumentSicovam(string Currency, string nameFormat, string feeName = null, string dstAccount = null, string srcAccount = null, string businessEvent = null, string counterparty = null, string allotment = null, int folio_id = 0, string fund = null, string commonIdentifier = null)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int retval = 0;
                logger.log(Severity.debug, "Trying to get cash instrument SICOVAM by name: " + Currency);
                if (!String.IsNullOrEmpty(Currency))
                {
                    if (!String.IsNullOrEmpty(nameFormat))
                    {
                        nameFormat = nameFormat.Replace("%CURRENCY%", Currency);
                    }
                    if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(feeName))
                    {
                        nameFormat = nameFormat.Replace("%FEETYPE%", feeName);
                    }
                    if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(dstAccount))
                    {
                        nameFormat = nameFormat.Replace("%DESTINATIONACCOUNT%", dstAccount);
                    }
                    if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(srcAccount))
                    {
                        nameFormat = nameFormat.Replace("%SOURCEACCOUNT%", srcAccount);
                    }
                    if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(businessEvent))
                    {
                        nameFormat = nameFormat.Replace("%BUSINESSEVENT%", businessEvent);
                    }
                    if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(counterparty))
                    {
                        nameFormat = nameFormat.Replace("%COUNTERPARTY%", counterparty);
                    }
                    if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(allotment))
                    {
                        nameFormat = nameFormat.Replace("%ALLOTMENT%", allotment);
                    }
                    if (!String.IsNullOrEmpty(nameFormat) && (!String.IsNullOrEmpty(fund) || folio_id != 0))
                    {
                        if (!String.IsNullOrEmpty(fund))
                        {
                            nameFormat = nameFormat.Replace("%FUND%", fund);
                        }
                        if (folio_id != 0)
                        {
                            int fund_folio = GetRootFundFolio(folio_id);
                            string reference = GetFundReference(fund_folio);
                            if (!String.IsNullOrEmpty(reference))
                            {
                                nameFormat = nameFormat.Replace("%FUND%", reference);
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(nameFormat) && !String.IsNullOrEmpty(commonIdentifier))
                    {
                        nameFormat = nameFormat.Replace("%EXTFUNDID%", commonIdentifier);
                        nameFormat = nameFormat.Replace("%COMMON_ID%", commonIdentifier);
                    }
                    try
                    {
                        string sql = "SELECT MAX(SICOVAM) FROM TITRES WHERE TYPE = 'C' AND STR_TO_DEVISE('" + Currency + "') = DEVISECTT AND LIBELLE like q'<%" + nameFormat + "%>'";
                        logger.log(Severity.debug, "About to run query: " + sql + ". Currency = " + Currency + ", nameFormat = " + nameFormat);
                        retval = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql));
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred while trying to get instrument from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return retval;
            }
        }

        protected bool CheckIfSicovamExists(int sicovam)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "Checking if SICOVAM exists: " + sicovam);
                bool res = false;
                int foundvalue = 0;
                if (sicovam != 0)
                {
                    try
                    {
                        string sql = "SELECT SICOVAM FROM TITRES WHERE SICOVAM = :SICOVAM";
                        OracleParameter parameter = new OracleParameter(":SICOVAM", sicovam);
                        List<OracleParameter> parameters = new List<OracleParameter>() {parameter};
                        int sicovamInt32 = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                        res = sicovamInt32 == sicovam;
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.debug,
                            "Error occurred while trying to check if sicovam exists in database: " + ex.Message +
                            ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return res;
            }
        }

        protected bool GetSicovamByName(string name, out int sicovam) //By name or by reference
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "Trying to get SICOVAM by name: " + name);
                bool res = false;
                sicovam = 0;
                if (!String.IsNullOrEmpty(name))
                {
                    try
                    {
                        string sql = "SELECT SICOVAM FROM TITRES WHERE LIBELLE = :LIBELLE OR REFERENCE = :REFERENCE";
                        OracleParameter parameter = new OracleParameter(":LIBELLE", name);
                        OracleParameter parameter1 = new OracleParameter(":REFERENCE", name);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameter, parameter1 };
                        sicovam = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                        if (sicovam > 0)
                        {
                            logger.log(Severity.debug, "Found an instrument by name: " + name + " sicovam = "+sicovam);
                            res = true;
                        }
                        else
                        {
                            logger.log(Severity.debug, "No instrument found by name: " + name);
                            res = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.warning, "Error occurred while trying to get sicovam by instrument name from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return res;
            }
        }

        protected int GetBussinessEvent(eCorporateActionType caType)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int res = 0;
                if (RBCCustomParameters.Instance.CorporateActionsBusinessEvents.ContainsKey(caType))
                {
                    if (_BusinessEvents.ContainsKey(RBCCustomParameters.Instance.CorporateActionsBusinessEvents[caType]))
                        res = _BusinessEvents[RBCCustomParameters.Instance.CorporateActionsBusinessEvents[caType]];
                    else
                        logger.log(Severity.warning, "Could not find bus event from cache by CA type" + Enum.GetName(typeof(eCorporateActionType), caType));
                }
                else
                    logger.log(Severity.warning, "Could not find bus event from cache by CA type" + Enum.GetName(typeof(eCorporateActionType), caType));
                return res;
            }
        }

        protected int GetCashBusinessEvent(string ExtFundId, string transactionType)
        {
            int businessEventID = 0;
            if (Regex.Match(ExtFundId, ".*COLIN").Success)
            {
                if (_BusinessEvents.ContainsKey(RBCCustomParameters.Instance.CollateralInBusinessEvent))
                    businessEventID = _BusinessEvents[RBCCustomParameters.Instance.CollateralInBusinessEvent];
            }
            else if (Regex.Match(ExtFundId, ".*COLOUT").Success)
            {
                if (_BusinessEvents.ContainsKey(RBCCustomParameters.Instance.CollateralOutBusinessEvent))
                    businessEventID = _BusinessEvents[RBCCustomParameters.Instance.CollateralOutBusinessEvent];
            }
            else if (RBCCustomParameters.Instance.InterestPaymentTypeNameList.Contains(transactionType.Trim().ToUpper()))
            {
                if (_BusinessEvents.ContainsKey(RBCCustomParameters.Instance.InterestPaymentBusinessEvent))
                    businessEventID = _BusinessEvents[RBCCustomParameters.Instance.InterestPaymentBusinessEvent];
            }
            else if (!String.IsNullOrEmpty(RBCCustomParameters.Instance.CashTransferBusinessEvent))
            {
                if (_BusinessEvents.ContainsKey(RBCCustomParameters.Instance.CashTransferBusinessEvent))
                    businessEventID = _BusinessEvents[RBCCustomParameters.Instance.CashTransferBusinessEvent];
            }
            return businessEventID;
        }

        protected int OverrideDefaultKernelWorkflow(ref ITicketMessage ticketMessage, string commonIdentifier)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int res = 0;
                if (commonIdentifier.ValidateNotEmpty())
                {
                    if (RBCCustomParameters.Instance.OverrideCreationEvent)
                    {
                        logger.log(Severity.debug, "OverrideCreationEvent parameter enebled");
                        if (IsMAMLCommonIdentifeir(commonIdentifier) && RBCCustomParameters.Instance.MAMLTradeCreationEventId != 0)
                        {
                            logger.log(Severity.debug, "Overriding creation event to MAMLTradeCreationEvent");
                            ticketMessage.SetTicketField(FieldId.CREATION_UPDATE_EVENT_ID, RBCCustomParameters.Instance.MAMLTradeCreationEventId);
                            res = RBCCustomParameters.Instance.MAMLTradeCreationEventId;
                        }
                        else if (RBCCustomParameters.Instance.DelegateTradeCreationEventId != 0)
                        {
                            logger.log(Severity.debug, "Overriding creation event to DelegateTradeCreationEvent");
                            ticketMessage.SetTicketField(FieldId.CREATION_UPDATE_EVENT_ID, RBCCustomParameters.Instance.DelegateTradeCreationEventId);
                            res = RBCCustomParameters.Instance.DelegateTradeCreationEventId;
                        }
                    }
                }
                return res;
            }
        }

        protected string GenerateSha1Hash(List<string> inputList, int ReversalFlagColumnID = -1)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "Trying to generate SHA1 hash");
                string retval = "";
                if (inputList == null)
                {
                    logger.log(Severity.debug, "NULL argument provided. Generating hash from current timestamp.");
                    return GenerateSha1Hash();
                }
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    string stringtohash = "";
                    if (ReversalFlagColumnID != -1)
                    {
                        logger.log(Severity.debug, "ReversalFlagColumnID is NOT -1");
                        StringBuilder strb = new StringBuilder();
                        for (int i = 0; i < inputList.Count; i++)
                        {
                            if (i != ReversalFlagColumnID)
                            {
                                strb.Append(inputList[i]);
                            }
                        }
                        stringtohash = strb.ToString();
                    }
                    else
                    {
                        stringtohash = String.Join("", inputList);
                    }
                    logger.log(Severity.debug, "String to hash = " + stringtohash);
                    byte[] sha1Hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(stringtohash));
                    retval = BitConverter.ToString(sha1Hash).Replace("-", "");
                }
                logger.log(Severity.debug, "SHA1 hash from CSV fields " + ((ReversalFlagColumnID != -1) ? ("[Ignored reversal flag]") : ("")) + " : " + retval);
                return retval;
            }
        }

        protected string GenerateSha1Hash()
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                string retval = "";
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    string timtstamp = DateTime.Now.ToString();
                    Random rnd = new Random();
                    timtstamp += rnd.Next();
                    byte[] sha1Hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(timtstamp));
                    retval = BitConverter.ToString(sha1Hash).Replace("-", "");
                }
                logger.log(Severity.debug, "SHA1 hash from timestamp: " + retval);
                return retval;
            }
        }

        #endregion
       
        #region Private
        private int GetStrategyFolioID(int RootFundFolioID, string strategyFolioName, string subStrategyFolioName = null)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetStrategyFolioID start");
                int res = 0;
                if (strategyFolioName.ValidateNotEmpty())
                {
                    try
                    {
                        if (subStrategyFolioName.ValidateNotEmpty())
                        {
                            logger.log(Severity.debug, "Trying to get GetStrategyFolioID ...");

                            string sql = "SELECT IDENT FROM FOLIO WHERE NAME = :subStrategyFolioName AND MGR IN (SELECT IDENT FROM FOLIO WHERE MGR = :RootFundFolioID AND NAME = :strategyFolioName)";
                            OracleParameter parameter = new OracleParameter(":subStrategyFolioName", subStrategyFolioName);
                            OracleParameter parameter1 = new OracleParameter(":RootFundFolioID", RootFundFolioID);
                            OracleParameter parameter2 = new OracleParameter(":strategyFolioName", strategyFolioName);
                            List<OracleParameter> parameters = new List<OracleParameter>() { parameter, parameter1, parameter2 };
                            res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                        }
                        else
                        {
                            string sql = "SELECT IDENT FROM FOLIO WHERE NAME like '%" + strategyFolioName + "%' AND MGR = :RootFundFolioID and rownum <= 1";
                            OracleParameter parameter = new OracleParameter(":RootFundFolioID", RootFundFolioID);
                            List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                            res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred while trying to get folio ID from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return res;
            }
        }

        private int GetPrimeBrokerBySicovam(int sicovam)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetPrimeBrokerBySicovam start");
                int res = 0;
                if (sicovam != 0)
                {
                    try
                    {
                        string sql = "select ident from fund_primebrokers where sicovam = :sicovam";
                        OracleParameter parameter = new OracleParameter(":sicovam", sicovam);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                        res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred while trying to get prime broker: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return res;
            }
        }

        private int GetFundSicovam(int folioID)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetFundSicovam start");
                int res = 0;
                if (folioID != 0)
                {
                    try
                    {
                        string sql = "select sicovam from funds where TRADINGFOLIO = :TRADINGFOLIO";
                        OracleParameter parameter = new OracleParameter(":TRADINGFOLIO", folioID);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                        res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred while trying to get fund sicovam: " + ex.Message + ". InnerException: " +
                                                                                                                 ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return res;
            }
        }
        
        private int GetRootFundFolio(int folioID)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetRootFundFolio start");
                int res = 0;
                if (folioID != 0)
                {
                    try
                    {
                        string sql = "select root_trading_folio from fund_folios where ident = :ident";
                        OracleParameter parameter = new OracleParameter(":ident", folioID);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                        res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred while trying to get entity: " + ex.Message + ". InnerException: " +
                                                                                                                 ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return res;
            }
        }

        private int GetPrimeBroker(string commonIdentifier)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                int res = 0;
                try
                {
                    if (_Rootportfolios.ContainsKey(commonIdentifier))
                    {
                        int rootFundFolio = GetRootFundFolio(_Rootportfolios[commonIdentifier]);
                        if (rootFundFolio > 0)
                        {
                            int sicovam = GetFundSicovam(rootFundFolio);
                            if (sicovam > 0)
                            {
                                res = GetPrimeBrokerBySicovam(sicovam);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while trying to get prime broker: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
                return res;
            }
        }

        private bool IsMAMLCommonIdentifeir(string commonIdentifier)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                if (commonIdentifier.ValidateNotEmpty())
                {
                    string last8chars = commonIdentifier.GetLast8Characters();
                    if (last8chars.ValidateNotEmpty())
                    {
                        logger.log(Severity.debug, "Checking if " + last8chars + " is a MAML Z Code");
                        if (_MAMLZCodesList.Contains(last8chars.ToUpper()))
                        {
                            logger.log(Severity.debug, last8chars + " is a MAML Z Code");
                            return true;
                        }
                        else
                            logger.log(Severity.debug, last8chars + " is not a MAML Z Code");
                    }
                }
                return false;
            }
        }

        private int GetEntityFromSWIFT(string BICCode)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetEntityFromSWIFT start");
                int res = 0;
                if (!String.IsNullOrEmpty(BICCode))
                {
                    try
                    {
                        string sql = "SELECT CODE FROM TIERSGENERAL WHERE SWIFT = :SWIFT";
                        OracleParameter parameter = new OracleParameter(":SWIFT", BICCode);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                        res = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred while trying to get entity by SWIFT from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return res;
            }
        }

        private string GetFundReference(int folioID)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.debug, "GetRootFundFolio start");
                string res = "";
                if (folioID != 0)
                {
                    try
                    {
                        string sql = "select reference from funds where TRADINGFOLIO = :TRADINGFOLIO";
                        OracleParameter parameter = new OracleParameter(":TRADINGFOLIO", folioID);
                        List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                        res = CSxDBHelper.GetOneRecord(sql, parameters).ToString();
                    }
                    catch (Exception ex)
                    {
                        logger.log(Severity.error, "Error occurred while trying to get fund reference: " + ex.Message + ". InnerException: " +
                            ex.InnerException + ". StackTrace: " + ex.StackTrace);
                    }
                }
                return res;
            }
        }
        #endregion

    }
}
