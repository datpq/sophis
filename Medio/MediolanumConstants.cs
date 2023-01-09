﻿using System;
using System.Collections.Generic;
using sophis.misc;
using sophis.utils;
using MEDIO.CORE.Tools;
using Oracle.DataAccess.Client;

namespace MEDIO
{
    public static class MedioConstants
    {
        // Criteria
        public const String MEDIO_USERCRITERIUM_FOASSETCLASS = "Classification FO Asset Class";

        // User right
        public const String MEDIO_USERRIGHT_HEDGING_VALIDATION = "Hedging Trade Validation";

        // WF 
        public const String MEDIO_WF_TRADECHECK_HEDGING = "Hedging Trade Check";

        // Widget 
        public const String MEDIO_WIDGET_HEDGINGVALIDATION = "Netting and Hedging Validation";

        // GUI elements resource id 
        public const int ID_ITEM_SHIFT = 100;
        public const int elementHedgeCheckID = 6501;
        public const int elementCommentID = 6510;
        public const int elementTradingHoursCheck = 6605 - ID_ITEM_SHIFT;
        public const int elementTradingHoursCheckLog = 6607 - ID_ITEM_SHIFT;
        public const int elementPARENT_ORDER_ID = 6610 - ID_ITEM_SHIFT;

        // Toolkit columns
        public const String MEDIO_COLUMN_STANDARD_NETCOMMITMENT = "Net Commitment";
        public const String MEDIO_COLUMN_STANDARD_NOMINALCCY1 = "Nominal 1st CCY";
        public const String MEDIO_COLUMN_STANDARD_NOMINALCCY2 = "Nominal 2nd CCY";

        // Histomvts column names
        public const String MEDIO_GUI_TRANSACTION_FIELD = "MEDIO_PARENTORDERID";

        // Standard columns
        public const String MEDIO_COLUMN_STD_UCITSCOMMITMENT = "UCITS Commitment";

        // Order properties 
        public const String MEDIO_ORDER_PROPERTY_CATEGORY_TKT = "Toolkit";
        public const String MEDIO_ORDER_PROPERTY_NAME_HEDGE = "Hedge Check";
        public const String MEDIO_ORDER_PROPERTY_NAME_HEDGECOMMENT = "Hedge Comment";
        public const String MEDIO_ORDER_PROPERTY_NAME_OUTSIDEPIT = "MEDIO_OUTSIDE_HRS";
        public const String MEDIO_ORDER_PROPERTY_PARENT_ORDERID = "MEDIO_PARENTORDERID";

        // Exec properties 
        public const String MEDIO_EXEC_PROPERTY_ISHEDGEDFUNDED = "MEDIO_IsHedgedFunded";

        // External Ref
        public const String MEDIO_MARKET_EXTREF_PITCLOSETIME = "Pit closing time (local)";
        public const String MEDIO_MARKET_EXTREF_TIMEZONE = "Timezone";

        public enum eActionTypy
        {
            No_Action = -1,
            Save = 0,
            Validation = 1,
            Refuse = 2,
            Creation = 3,
            Modification = 4
        }

        public enum eHedgeStatus
        {
            No_Status = -1,
            Not_Hedging = 0,
            Hedging = 1,
            Validated = 2,
            Refused = 3
        }

        public enum EOrderExecutionState
        {
            Unkwnown = 0,
            PartiallyExecuted = 1,
            TotallyExecuted = 2
        }
    }

    namespace MEDIO_CUSTOM_PARAM
    {
        public class HEDGING_FUNDING_ORDERS
        {
            #region const
            protected const string MEDIO_OMS_CUSTOM_SECTION = "MEDIO_OMS_HEDGING";
            protected const string MEDIO_ENTRY_initial_wf_state = "initial_wf_state";
            protected const string MEDIO_DFLT_initial_wf_state = "Totally Executed";
            protected const string MEDIO_ENTRY_wf_event = "wf_event";
            protected const string MEDIO_DFLT_wf_event = "OrderCustomEvent1";
            protected const string MEDIO_ENTRY_inMkt_wf_state = "inMkt_wf_state";
            protected const string MEDIO_DFLT_inMkt_wf_state = "In Market";
            protected const string MEDIO_ENTRY_bucketSet_Name = "BucketSet";
            protected const string MEDIO_DFLT_Bucket_Name = "Forward Dates";
            protected const string MEDIO_ENTRY_HEDGING_STRAT = "HedgingOriginationStrategy";
            protected const string MEDIO_DFLT_HEDGING_STRAT = "MEDIO Hedging/Funding";
            #endregion

            #region Singleton

            private static HEDGING_FUNDING_ORDERS gInstance;

            public static HEDGING_FUNDING_ORDERS Instance
            {
                get
                {
                    if (gInstance == null)
                        gInstance = new HEDGING_FUNDING_ORDERS();
                    return gInstance;
                }
            }

            private HEDGING_FUNDING_ORDERS()
            {
                string _initial_wf_state = "";
                string _wf_event = "";
                string _inMkt_wf_state = "";
                string _bucketSet_Name = "";
                string _originationStrat = "";

                CSMConfigurationFile.getEntryValue(MEDIO_OMS_CUSTOM_SECTION, MEDIO_ENTRY_initial_wf_state, ref _initial_wf_state, MEDIO_DFLT_initial_wf_state);
                CSMConfigurationFile.getEntryValue(MEDIO_OMS_CUSTOM_SECTION, MEDIO_ENTRY_wf_event, ref _wf_event, MEDIO_DFLT_wf_event);
                CSMConfigurationFile.getEntryValue(MEDIO_OMS_CUSTOM_SECTION, MEDIO_ENTRY_inMkt_wf_state, ref _inMkt_wf_state, MEDIO_DFLT_inMkt_wf_state);
                CSMConfigurationFile.getEntryValue(MEDIO_OMS_CUSTOM_SECTION, MEDIO_ENTRY_bucketSet_Name, ref _bucketSet_Name, MEDIO_DFLT_Bucket_Name);
                CSMConfigurationFile.getEntryValue(MEDIO_OMS_CUSTOM_SECTION, MEDIO_ENTRY_HEDGING_STRAT, ref _originationStrat, MEDIO_DFLT_HEDGING_STRAT);

                Initial_wf_state = _initial_wf_state;
                WF_event = _wf_event;
                InMkt_wf_state = _inMkt_wf_state;
                BucketSetModelName = _bucketSet_Name;
                OriginationStrat = GetOriginationStategy(_originationStrat);
            }
            #endregion

            #region Getters

            public string Initial_wf_state
            {
                get;
                protected set;
            }
            public string InMkt_wf_state
            {
                get;
                protected set;
            }
            public string WF_event
            {
                get;
                protected set;
            }
            public string BucketSetModelName
            {
                get;
                protected set;
            }

            public int OriginationStrat
            {
                get;
                protected set;
            }
            #endregion

            private int GetOriginationStategy(string name)
            {
                string sql = "select id from Order_originationstrategy where name = :name";
                OracleParameter parameter = new OracleParameter(":name", name);
                List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                return Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
            }
        }

        public class FXROLL_ORDERS
        {
            protected const string MEDIO_OMS_CUSTOM_SECTION = "MEDIO_OMS_FXROLL";
            protected const string MEDIO_ENTRY_FXROLL_STRAT = "FXROLLOriginationStrategy";

            public int OriginationStrat;

            private static FXROLL_ORDERS gInstance;
            public static FXROLL_ORDERS Instance
            {
                get
                {
                    if (gInstance == null) gInstance = new FXROLL_ORDERS();
                    return gInstance;
                }
            }

            private FXROLL_ORDERS()
            {
                GetOriginationStategy();
            }

            private void GetOriginationStategy()
            {
                string _originationStat = "";
                CSMConfigurationFile.getEntryValue(MEDIO_OMS_CUSTOM_SECTION, MEDIO_ENTRY_FXROLL_STRAT, ref _originationStat, "MEDIO FX Rolling");
                string sql = "select id from Order_originationstrategy where name = :name";
                OracleParameter parameter = new OracleParameter(":name", _originationStat);
                List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
                this.OriginationStrat = Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
            }

        }



        public class CSxToolkitCustomParameter
        {
            #region const
            private static object syncObj = new Object();
            private const string MEDIO_BO_CONDITION_CUSTOM_SECTION = "MEDIO_BACKOFFICE_CONDITION";
            private const string MEDIO_BO_MATCHINGLAW_CUSTOM_ENTRY = "Thirdparty agreement";

            private const string MEDIO_BO_THIRDPARTY_CUSTOM_SECTION = "MEDIO_THIRDPARTY_CONDITION";
            private const string MEDIO_BO_THIRDPARTY_SETTLEPLACE_ENTRY = "Settlement Place Reference";

            private const string MEDIO_BO_KERNEL_EVENT_Delegate_Upload = "Delegate Upload event";
            private const string MEDIO_BO_KERNEL_EVENT_MAML_Upload = "MAML Upload event";

            private const string MEDIO_OMS_WF_CUSTOM_SECTION = "MEDIO_OMS_WF";
            private const string MEDIO_OMS_WF_PORTFOLIONAME_ENTRY = "Portfolio name match";

            private const string MEDIO_USERGROUP_FXEXCEPTION_SECTION = "MEDIO_USERGROUP";
            private const string MEDIO_USERGROUP_FXEXCEPTION_ENTRY = "FX EXCEPTION group name";

            private const string MEDIO_USERGROUP_FXDEFAULT_SECTION = "MEDIO_USERGROUP";
            private const string MEDIO_USERGROUP_FXDEFAULT_ENTRY = "FX DEFAULT group name";

            private const string MEDIO_BO_ALLOTMENT_FUND_LEGAL_SECTION = "ALLOTMENT_GFPFund";
            private const string MEDIO_BO_ALLOTMENT_FUND_LEGAL_ENTRY = "LEGAL_FORM";

            private const string MEDIO_BO_TREASURY_ACCOUNT_EMAIL_REF_SECTION = "BO_Account_Extenal_Ref";
            private const string MEDIO_BO_TREASURY_ACCOUNT_EMAIL_REF_ENTRY = "DelegateMangerEmail";

            protected const string MEDIO_BO_CHECKDEAL = "MEDIO_BACKOFFICE_CHECKDEAL";
            protected const string MEDIO_ENTRY_checkOperator_orderWFStatus = "CheckOperatorOrderWFStatus";

            protected const string MEDIO_RMA_CUSTOM_SECTION = "MediolanumRMA";

            #endregion

            // TO DO QUERY DB


            #region Value
            public string BO_THIRDPARTY_AGREEMENT = "Electronic Matching";
            public string BO_THIRDPARTY_SETTLEPLACEREF = "PLACE OF SETT REF";
            public string BO_EVENT_DELEGATE_UPLOAD = "Delegate Upload";
            public string BO_EVENT_MAML_UPLOAD = "MAML Upload";
            public string OMS_WF_PORTFOLIONAME = CSxDBHelper.GetTargetTradingFolio();//"MAML"; //TOCHANGE
            public string USERGROUP_FXEXCEPTION_NAME = "Treasury Trader";
            public string USERGROUP_FXDEFAULT_NAME = "Derivatives trader";
            public string FUND_LEGALFORM = "Open-End Fund";
            public string BO_ACCOUNT_EMAIL_EXT_REF = "DelegateManagerEmail";
            public string OMS_WF_OPERATORCHECK = "Send To Execute";
            public string RMAGenericTradeOperators;

            #endregion

            private static CSxToolkitCustomParameter gInstance;
            public static CSxToolkitCustomParameter Instance
            {
                get
                {
                    if (gInstance == null)
                    {
                        lock (syncObj) // thread safe
                        {
                            gInstance = new CSxToolkitCustomParameter();
                        }
                    }
                    return gInstance;
                }
            }

            private CSxToolkitCustomParameter()
            {
                CSMConfigurationFile.getEntryValue(MEDIO_BO_CONDITION_CUSTOM_SECTION, MEDIO_BO_MATCHINGLAW_CUSTOM_ENTRY,
                    ref BO_THIRDPARTY_AGREEMENT, BO_THIRDPARTY_AGREEMENT);

                CSMConfigurationFile.getEntryValue(MEDIO_BO_CONDITION_CUSTOM_SECTION, MEDIO_BO_KERNEL_EVENT_Delegate_Upload,
                    ref BO_EVENT_DELEGATE_UPLOAD, BO_EVENT_DELEGATE_UPLOAD);

                CSMConfigurationFile.getEntryValue(MEDIO_BO_CONDITION_CUSTOM_SECTION, MEDIO_BO_KERNEL_EVENT_MAML_Upload,
                    ref BO_EVENT_MAML_UPLOAD, BO_EVENT_MAML_UPLOAD);

                CSMConfigurationFile.getEntryValue(MEDIO_BO_THIRDPARTY_CUSTOM_SECTION, MEDIO_BO_THIRDPARTY_SETTLEPLACE_ENTRY,
                    ref BO_THIRDPARTY_SETTLEPLACEREF, BO_THIRDPARTY_SETTLEPLACEREF);

                CSMConfigurationFile.getEntryValue(MEDIO_OMS_WF_CUSTOM_SECTION, MEDIO_OMS_WF_PORTFOLIONAME_ENTRY,
                    ref OMS_WF_PORTFOLIONAME, OMS_WF_PORTFOLIONAME);

                CSMConfigurationFile.getEntryValue(MEDIO_USERGROUP_FXEXCEPTION_SECTION, MEDIO_USERGROUP_FXEXCEPTION_ENTRY,
                    ref USERGROUP_FXEXCEPTION_NAME, USERGROUP_FXEXCEPTION_NAME);

                CSMConfigurationFile.getEntryValue(MEDIO_USERGROUP_FXDEFAULT_SECTION, MEDIO_USERGROUP_FXDEFAULT_ENTRY,
                    ref USERGROUP_FXDEFAULT_NAME, USERGROUP_FXDEFAULT_NAME);

                CSMConfigurationFile.getEntryValue(MEDIO_BO_ALLOTMENT_FUND_LEGAL_SECTION, MEDIO_BO_ALLOTMENT_FUND_LEGAL_ENTRY,
                    ref FUND_LEGALFORM, FUND_LEGALFORM);

                CSMConfigurationFile.getEntryValue(MEDIO_BO_TREASURY_ACCOUNT_EMAIL_REF_SECTION, MEDIO_BO_TREASURY_ACCOUNT_EMAIL_REF_ENTRY,
                    ref BO_ACCOUNT_EMAIL_EXT_REF, BO_ACCOUNT_EMAIL_EXT_REF);

                CSMConfigurationFile.getEntryValue(MEDIO_BO_CHECKDEAL, MEDIO_ENTRY_checkOperator_orderWFStatus,
                    ref OMS_WF_OPERATORCHECK, OMS_WF_OPERATORCHECK);

                CSMConfigurationFile.getEntryValue(MEDIO_RMA_CUSTOM_SECTION, "RMAGenericTradeOperators", ref RMAGenericTradeOperators, "BBHUploader");
            }


        }
    }
}


