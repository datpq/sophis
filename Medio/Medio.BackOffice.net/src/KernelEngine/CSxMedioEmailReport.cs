using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using Oracle.DataAccess.Client;
using sophis.portfolio;
using sophis.static_data;
using sophis.utils;
using MEDIO.MEDIO_CUSTOM_PARAM;
using Sophis.OMS.Util;

namespace MEDIO.BackOffice.net.src.KernelEngine
{
    public enum ECashType
    {
        UnDefined = 0,
        Subscription,
        Redemption
    }

    class CSxMedioEmailReport
    {
        #region Fields
        public long TradeId
        {
            get { return _trade.getInternalCode(); }
        }
        public ECashType Type { get; private set; }
        public string MangerName { get; private set; }
        public string FundName { get; private set; }
        public string Currency { get; private set; }
        public double Amount { get; private set; }
        public DateTime TradeDate { get; private set; }
        public DateTime SettlementDate { get; private set; }
        public string DebitCustodyName { get; private set; }
        public string DebitCustodyAccount { get; private set; }
        public string CreditCustodyName { get; private set; }
        public string CreditCustodyAccount { get; private set; }
        #endregion

        private readonly CSMTransaction _trade;

        public CSxMedioEmailReport(CSMTransaction trade)
        {
            _trade = trade;
            SetFundName();
            SetSettlementCcy();
            SetAmount();
            SetDates();
            DebitAccount();
            CreditAccount();
            SetCashType();
            SetManagerName();
        }

        private void SetCashType()
        {
            if (DebitCustodyName.ToUpper().Contains(CSxDBHelper.GetBOCashAccountNamePattern()))//"MAML"))//not MIFL (Trading Folio)
                Type = ECashType.Subscription;
            else if (CreditCustodyName.ToUpper().Contains(CSxDBHelper.GetBOCashAccountNamePattern()))//"MAML"))//not MIFL (Trading Folio)
                Type = ECashType.Redemption;
            else 
                Type = ECashType.UnDefined;
        }

        private void SetManagerName()
        {
            if (Type == ECashType.UnDefined) SetCashType();
            if (Type == ECashType.Subscription) MangerName = GetAccountName(_trade.GetLostroCashId());
            else if (Type == ECashType.Redemption) MangerName = GetAccountName(_trade.GetNostroCashId());
            MangerName = String.IsNullOrEmpty(MangerName) ? MangerName : MangerName.Substring(MangerName.LastIndexOf('-') + 1); 
        }

        private void SetFundName()
        {
            FundName = CSxUtils.GetFundName(_trade.GetFolioCode());
        }

        private void SetSettlementCcy()
        {
            CMString ccy = "";
            CSMCurrency.CurrencyToString(_trade.GetSettlementCurrency(), ccy);
            Currency = ccy;
        }

        private void SetAmount()
        {
            Amount = _trade.GetNetAmount();
        }

        private void SetDates()
        {
            TradeDate = CSxUtils.GetDateFromSophisTime(_trade.GetTransactionDate());
            SettlementDate = CSxUtils.GetDateFromSophisTime(_trade.GetSettlementDate());
        }

        private void DebitAccount()
        {
            DebitCustodyAccount = GetAccountAtCustodian(_trade.GetNostroCashId());
            DebitCustodyName = GetAccountName(_trade.GetNostroCashId());
        }

        private void CreditAccount()
        {
            CreditCustodyAccount = GetAccountAtCustodian(_trade.GetLostroCashId());
            CreditCustodyName = GetAccountName(_trade.GetLostroCashId());
        }

        #region Queries 
        private string GetAccountName(int id)
        {
            if (id <= 0) return "";
            string sql = String.Format("select ACCOUNT_NAME from BO_SSI_PATH where SSI_PATH_ID = {0}", id);
            return CSxDBHelper.GetOneRecord<string>(sql);
        }

        private string GetAccountAtCustodian(int id)
        {
            if (id <= 0) return "";
            string sql = String.Format("select ACCOUNT_AT_CUSTODIAN from BO_SSI_PATH where SSI_PATH_ID = {0}", id);
            return CSxDBHelper.GetOneRecord<string>(sql);
        }

        public List<string> GetDelegateMangersEmailFromAccount()
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Begin(typeof(CSxMedioEmailReport).Name, MethodBase.GetCurrentMethod().Name);
                var emailList = new List<string>();

                logger.Write(CSMLog.eMVerbosity.M_debug, "Getting email address from lostro account external ref ... ");
                var emailStr = GetAccountEmailExternalRefFromDB(_trade.GetLostroCashId());
                foreach (var oneEmail in ParseString(emailStr, ';'))
                {
                    if (!emailList.Contains(oneEmail))
                    {
                        emailList.Add(oneEmail);
                        logger.Write(CSMLog.eMVerbosity.M_debug, "Adding email address : " + oneEmail + ". Found from account " + _trade.GetLostroCashId());
                    }
                }

                logger.Write(CSMLog.eMVerbosity.M_debug, "Getting email address from nostro account external ref ... ");
                emailStr = GetAccountEmailExternalRefFromDB(_trade.GetNostroCashId());
                foreach (var oneEmail in ParseString(emailStr, ';'))
                {
                    if (!emailList.Contains(oneEmail))
                    {
                        emailList.Add(oneEmail);
                        logger.Write(CSMLog.eMVerbosity.M_debug, "Adding email address : " + oneEmail + ". Found from account " + _trade.GetNostroCashId());
                    }
                }

                if (emailList.IsNullOrEmpty())
                    logger.Write(CSMLog.eMVerbosity.M_debug, "No email address has found from both account " + _trade.GetNostroCashId() + " and " + _trade.GetLostroCashId());

                return emailList;
            }
        }

        private string GetAccountEmailExternalRefFromDB(int ssiPathId)
        {
            using (CSMLog logger = new CSMLog())
            {
                logger.Begin(typeof(CSxMedioEmailReport).Name, MethodBase.GetCurrentMethod().Name);

                string sql = "Select REF.VALUE"
                      + " from BO_TREASURY_EXT_REF REF,"
                      + " BO_TREASURY_EXT_REF_DEF DEF,"
                      + " BO_SSI_PATH             PATH"
                      + " WHERE DEF.REF_NAME     = :REF_NAME"
                      + " AND  REF.REF_ID        = DEF.REF_ID"
                      + " AND  REF.ACC_ID        = PATH.BO_TREASURY_ACCOUNT_ID"
                      + " AND  PATH.SSI_PATH_ID  = :SSI_PATH_ID";

                OracleParameter parameter = new OracleParameter(":REF_NAME", CSxToolkitCustomParameter.Instance.BO_ACCOUNT_EMAIL_EXT_REF);
                OracleParameter parameter1 = new OracleParameter(":SSI_PATH_ID", ssiPathId);
                var parameters = new List<OracleParameter>() { parameter, parameter1 };
                var res = CSxDBHelper.GetOneRecord<string>(sql, parameters);
                logger.Write(CSMLog.eMVerbosity.M_debug, "Returned value = " + res);
                return res;
            }
        }

        private string[] ParseString(string str, char delimiter)
        {
            if (String.IsNullOrEmpty(str)) return new string[]{};
            return str.Split(delimiter);
        }   
        #endregion
      
    }
}
