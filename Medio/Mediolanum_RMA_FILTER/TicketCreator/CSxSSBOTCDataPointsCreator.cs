using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mediolanum_RMA_FILTER.TicketCreator.AbstractBase;
using RichMarketAdapter.ticket;
using Mediolanum_RMA_FILTER.Tools;
using System.Globalization;
using sophis.log;
using System.Reflection;
using Sophis.DataAccess;
using MEDIO.CORE.Tools;
using transaction;

namespace Mediolanum_RMA_FILTER
{
    public class IngestionType
    {
        public const string CC23 = "CC23";
        public const string MIOTC23 = "MIOTC23";
        public const string CSAPoint = "CSA.";
    }
    
    class CSxSSBOTCDataPointsCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxSSBOTCDataPointsCreator).Name;
        private static string FundCodesToIgnoreInCaseOfCSA = "";
        public static string _MIFLFundCodes = "";
        public static string InScopeFundCodes = "";

        public CSxSSBOTCDataPointsCreator(eRBCTicketType ticketType) : base(ticketType) { }
        
        static CSxSSBOTCDataPointsCreator()
        {
            CheckIfIngestionTypeIsDim();
            InitializeIngestionTypesAreActives();
            GetFundCodesInScope();
            GetSleeveFundCodes();
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                base.SetTicketMessage(ref ticketMessage, fields);

                string ExtFundId = fields.GetValue(RBCTicketType.SSBOTCDataPointsColumns.FundCode);
                string _CusipPrefix = fields.GetValue(RBCTicketType.SSBOTCDataPointsColumns.CUSIPPrefix);
                DateTime _Date = DateTime.ParseExact(fields.GetValue(RBCTicketType.SSBOTCDataPointsColumns.Date), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                double _NetAmount = fields.GetValue(RBCTicketType.SSBOTCDataPointsColumns.NetAmount).ToDouble();
                logger.log(Severity.debug, $"Processing the line : ExtFundId={ExtFundId}, _CusipPrefix={_CusipPrefix}, _Date={_Date}, _NetAmount={_NetAmount}");
                
                if (!InScopeFundCodes.Contains(ExtFundId))
                {
                    logger.log(Severity.warning, $"{ExtFundId} ignored as it doen't belong to the fund codes that are in scope : '{InScopeFundCodes}'");
                    return true;
                }
                if (!IngestionTypesAreActive[_CusipPrefix] || !(IngesTypesAndTheirStatus[_CusipPrefix] == "BOTH" || (IngesTypesAndTheirStatus[_CusipPrefix] == "DIM" && !_MIFLFundCodes.Contains(ExtFundId)) ||
                    (IngesTypesAndTheirStatus[_CusipPrefix] == "MIFL" && _MIFLFundCodes.Contains(ExtFundId))))
                {
                    logger.log($"Line : {string.Join(";", fields)} Ignored due to MIFL Code List : {_MIFLFundCodes} OR because of the status of the Cusip Prefix {_CusipPrefix}");
                    return true;
                }
                else
                {
                    logger.log(Severity.debug, $"Starting Creating the Business Event Ticket. the Manager Group of _CusipPrefix={_CusipPrefix} is {IngesTypesAndTheirStatus[_CusipPrefix]}");
                    switch (_CusipPrefix)
                    {
                        case IngestionType.CC23:
                            //book a ‘CC Colateral’ business event ticket in Fusion.
                            ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, _BusinessEvents["CC Colateral"]);
                            int CC23folioID = SetCashFolioID(ref ticketMessage, ExtFundId + "COLOUTCC");
                            int CC23csicovam = GetCashInstrumentSicovam("EUR", RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, ExtFundId, ExtFundId, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, CC23folioID, null, ExtFundId);
                            ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, CC23csicovam);

                            break;
                        case IngestionType.MIOTC23:
                            //book a ‘Initial Margin CC’ business event ticket in Fusion
                            ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, _BusinessEvents["Initial Margin CC"]);
                            int MiotcfolioID = SetCashFolioID(ref ticketMessage, ExtFundId + "COLOUTIMCC");
                            int Miotcsicovam = GetCashInstrumentSicovam("EUR", RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, ExtFundId, ExtFundId, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, MiotcfolioID, null, ExtFundId);
                            ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, Miotcsicovam);
                            break;
                        case IngestionType.CSAPoint:
                            //book a ‘Collateral Out’ business event ticket in Fusion
                            ticketMessage.SetTicketField(FieldId.TRADETYPE_PROPERTY_NAME, _BusinessEvents["Collateral Out"]);
                            int CSAfolioID = SetCashFolioID(ref ticketMessage, FundCodesToIgnoreInCaseOfCSA.Contains(ExtFundId) ? ExtFundId : ExtFundId + "COLOUT");
                            int CSAsicovam = GetCashInstrumentSicovam("EUR", RBCCustomParameters.Instance.CashTransferInstrumentNameFormat, null, ExtFundId, ExtFundId, RBCCustomParameters.Instance.CashTransferBusinessEvent, RBCCustomParameters.Instance.DefaultCounterpartyStr, null, CSAfolioID, null, ExtFundId);
                            ticketMessage.SetTicketField(FieldId.INSTRUMENTREF_PROPERTY_NAME, CSAsicovam);
                            break;
                        default:
                            break;
                    }
                   
                    ticketMessage.SetTicketField(FieldId.QUANTITY_PROPERTY_NAME, _NetAmount);
                    int tradeDate = _Date.ToSophisDate();
                    ticketMessage.SetTicketField(FieldId.NEGOTIATIONDATE_PROPERTY_NAME, tradeDate);
                    ticketMessage.SetTicketField(FieldId.SETTLDATE_PROPERTY_NAME, tradeDate);
                    int spotType = SpotTypeConstants.IN_PRICE;
                    ticketMessage.SetTicketField(FieldId.SPOTTYPE_PROPERTY_NAME, spotType);
                    ticketMessage.SetTicketField(FieldId.SPOT_PROPERTY_NAME, 1);

                    SetDepositary(ref ticketMessage, ExtFundId);
                    SetDefaultKernelWorkflow(ref ticketMessage, ExtFundId);
                    string generatedHash = GenerateSha1Hash(fields);
                    ticketMessage.SetTicketField(FieldId.EXTERNALREF_PROPERTY_NAME, generatedHash);

                    return false;
                }
            }

        }

        private static void GetFundCodesInScope()
        {
            //It's Called one time for the initialization
            Logger logger = new Logger(_ClassName, nameof(GetFundCodesInScope));
            if (DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = $"select fa_fund_code, custody_fund_code from {RBCCustomParameters.Instance.SSBFundCodesTableName}";
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0)) InScopeFundCodes += reader.GetString(0) + ",";
                    if (!reader.IsDBNull(1)) InScopeFundCodes += reader.GetString(1) + ",";
                }
                logger.log(Severity.info, $"The Fund Codes in Scope are : {InScopeFundCodes}");
            }
            logger.Dispose();
        }
        private static void GetSleeveFundCodes()
        {
            Logger logger = new Logger(_ClassName, nameof(GetSleeveFundCodes));
            if (DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = "select custody_fund_code from " + RBCCustomParameters.Instance.SSBFundCodesTableName + " where custody_fund_code is not null";
                logger.log(Severity.info, $"Executing the query : {cmd.CommandText}");
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0)) FundCodesToIgnoreInCaseOfCSA += reader.GetString(0) + ",";
                }
                logger.log(Severity.info, $"Result of Query {FundCodesToIgnoreInCaseOfCSA}");
            }
            logger.Dispose();
        }

        private static Dictionary<string, bool> IngestionTypesAreActive = new Dictionary<string, bool>();
        private static void InitializeIngestionTypesAreActives()
        {
            if (IngestionTypesAreActive.Any()) return;
            Logger logger = new Logger(_ClassName, nameof(InitializeIngestionTypesAreActives));
            if (DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using (var cmd = DBContext.Connection.CreateCommand())
            {
                cmd.CommandText = "select cusip_prefix, active from " + RBCCustomParameters.Instance.SSBDataPointsTableName;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                    {
                        string IngesType = reader.GetString(0);
                        bool isActive = reader.GetString(1) == "1" ? true : false;
                        IngestionTypesAreActive[IngesType] = isActive;
                    }
                }
            }
        }
        private static Dictionary<string, string> IngesTypesAndTheirStatus = new Dictionary<string, string>();
        private static void CheckIfIngestionTypeIsDim()
        {
            if (IngesTypesAndTheirStatus.Any()) return;
            if (DBContext.Connection == null) CSxDBHelper.InitDBConnection();
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                try
                {
                    using (var cmd = DBContext.Connection.CreateCommand())
                    {
                        cmd.CommandText = "select * from " + RBCCustomParameters.Instance.SSBDataPointsTableName;
                        logger.log(Severity.debug, $"cmd.CommandText={cmd.CommandText}");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader[0] != DBNull.Value && reader[1] != DBNull.Value) //We can limit these values after to the ones that we have in the Scope
                                {
                                    var ingestionType = reader.GetString(0);
                                    var managerGroup = reader.GetString(1).ToUpper();
                                    //managerGroup.Equals("DIM") || managerGroup.Equals("BOTH")
                                    IngesTypesAndTheirStatus.Add(ingestionType, managerGroup);
                                    logger.log(Severity.debug, $"Ingestion Type: {ingestionType}---{managerGroup}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.log(Severity.error, "Error occurred while Checking IngestionTypes from database: " + ex.Message + ". InnerException: " + ex.InnerException + ". StackTrace: " + ex.StackTrace);
                }
            }
        }
    }
}
