using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using MEDIO.OrderAutomation.net.Source.Data;
using Oracle.DataAccess.Client;
using sophis.instrument;
using sophis.oms;
using sophis.portfolio;
using sophis.utils;
using sophis.value;
using Sophis.Data.Utils;

namespace MEDIO.OrderAutomation.net.Source.Criteria
{
    public class CSxHedgingFundingCriterium : CSMCriterium
    {
        private readonly string fColumnName = "";

        private static Dictionary<int, List<int>> _orderIdSicovamMap;
        private static Dictionary<int/*sophisOrderID*/, List<int/*sicovam*/>> OrderIdSicovamMap
        {
            get
            {
                if (_orderIdSicovamMap == null)
                {
                    InitOrderSicovamList();
                }
                return _orderIdSicovamMap;
            }
        }

        private static Dictionary<long, List<int>> _parentOrderTradesMap;
        private static Dictionary<long/*refcon*/, List<int/*MEDIO_ParentOrderID*/>> ParentOrderTradesMap
        {
            get
            {
                if (_parentOrderTradesMap == null)
                {
                    InitTradeIDParentOrderList();
                }
                return _parentOrderTradesMap;
            }
            set { }
        }

        public CSxHedgingFundingCriterium(string columnName) : base()
        {
            fColumnName = columnName;
        }

        public CSxHedgingFundingCriterium()
            : base()
        {
        }

        public override CSMCriterium Clone()
        {
            return new CSxHedgingFundingCriterium(fColumnName);
        }

        public override CSMCriterium.MCriterionCaps GetCaps()
        {
            return new MCriterionCaps(true, false, false);
        }

        public override void GetCode(SSMReportingTrade mvt, ArrayList list)
        {
            using (var log = new CSMLog())
            {
                log.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                list.Clear();
                SSMOneValue value = new SSMOneValue();
                if (IsFXDeal(mvt.sicovam))
                {
                    log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Trade {0} is a fx deal", mvt.refcon));
                    int parentOrder = GetParentOrderID(mvt.refcon);
                    if (parentOrder != 0)
                    {
                        log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Trade {0} has a parent trade with order id = {1}", mvt.refcon, parentOrder));
                        if (!OrderIdSicovamMap.ContainsKey(parentOrder))
                        {
                            OrderIdSicovamMap[parentOrder] = new List<int>();
                            OrderIdSicovamMap[parentOrder].Add(GetSicovam(parentOrder));
                        }
                        int sico = OrderIdSicovamMap[parentOrder].FirstOrDefault();
                        if (sico != 0)
                        {
                            log.Write(CSMLog.eMVerbosity.M_debug, String.Format("Parent sicovam #{0} of trade {1} has been fund! Adding the trade to parent position bucket", sico, mvt.refcon));
                            value.fCode = sico;
                            list.Add(value);
                        }
                    }
                    else
                    {
                        log.Write(CSMLog.eMVerbosity.M_warning, String.Format("Trade {0} does not have a parent trade!", mvt.refcon));
                    }
                }
                else
                {
                    value.fCode = mvt.sicovam;
                    list.Add(value);
                }
                log.End();
            }
        }

        public override void GetName(int code, CMString name, long size)
        {
            CSMInstrument inst = CSMInstrument.GetInstance(code);
            if (inst != null)
            {
                CMString instName = inst.GetName();
                name.StringValue = "Hedging & Funding " + instName;
                size = name.StringValue.Length;
            }
        }
        
        public static void InitTradeIDParentOrderList()
        {
            string sql = "select refcon, MEDIO_PARENTORDERID from join_position_histomvts";
            _parentOrderTradesMap = CSxDBHelper.GetMultiRecordDictionary<long, Int32>(sql);
        }
        
        public static void InitOrderSicovamList()
        {
            string sql = "select SOPHIS_ORDER_ID, sicovam from join_position_histomvts";
            _orderIdSicovamMap = CSxDBHelper.GetMultiRecordDictionary<Int32, Int32>(sql);
        }

        public static int GetSicovam(int orderID)
        {
            string sql = String.Format("select sicovam from join_position_histomvts where SOPHIS_ORDER_ID = :SOPHIS_ORDER_ID");
            OracleParameter parameter = new OracleParameter(":SOPHIS_ORDER_ID", orderID);
            List<OracleParameter> parameters = new List<OracleParameter>() { parameter };
            return Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
        }

        public static int GetTradeParentOrderID(long refcon)
        {
            string sql = "select MEDIO_PARENTORDERID from join_position_histomvts where refcon = :refcon";
            OracleParameter parameter = new OracleParameter("refcon", refcon);
            List<OracleParameter> parameters = new List<OracleParameter>() { parameter};
            return Convert.ToInt32(CSxDBHelper.GetOneRecord(sql, parameters));
        }

        public static bool IsFXDeal(int sicovam)
        {
            using (var LOG = new CSMLog())
            {
                bool res = false;
                LOG.Begin("CSxHedgingFundingCriterium", MethodBase.GetCurrentMethod().Name);
                CSMInstrument inst = CSMInstrument.GetInstance(sicovam);
                if (inst != null)
                {
                    CSMForexSpot forex = CSMForexSpot.GetInstance(sicovam);
                    CSMNonDeliverableForexForward ndf = CSMNonDeliverableForexForward.GetInstance(sicovam);
                    CSMForexFuture fxFwd = CSMForexFuture.GetInstance(sicovam);
                    bool isFXForward = false;
                    if (fxFwd != null) isFXForward = fxFwd.GetCurrencyCode() != fxFwd.GetExpiryCurrency();
                    res = (forex != null || ndf != null || isFXForward);
                    LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Sicovam #{0} of type {1}. Is it an FX? {2}", sicovam, inst.GetType_API(), res.ToString()));
                }
                return res;
            }
        }

        protected int GetParentOrderID(long refcon)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                if (!ParentOrderTradesMap.ContainsKey(refcon))
                {
                    ParentOrderTradesMap[refcon] = new List<int>();
                    int parentID = GetTradeParentOrderID(refcon);
                    ParentOrderTradesMap[refcon].Add(parentID);
                    LOG.Write(CSMLog.eMVerbosity.M_debug, String.Format("Trade {0} Medio parent order id is fetched = {1}", refcon, parentID));
                }
                LOG.End();
                return ParentOrderTradesMap[refcon].FirstOrDefault();
            }
        }

    }
}
