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
    public class CSxParentOrderIDCriterium : CSxHedgingFundingCriterium
    {
        private const string fClassName = "CSxParentOrderIDCriterium";
        private string fColumnName = "";

        public CSxParentOrderIDCriterium(string columnName)
            : base()
        {
            fColumnName = columnName;
        }

        public CSxParentOrderIDCriterium()
            : base()
        {
        }

        public override CSMCriterium Clone()
        {
            return new CSxParentOrderIDCriterium(fColumnName);
        }

        public override CSMCriterium.MCriterionCaps GetCaps()
        {
            return new MCriterionCaps(true, false, false);
        }

        public override void GetCode(SSMReportingTrade mvt, ArrayList list)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);
                list.Clear();
                SSMOneValue value = new SSMOneValue();
                
                int parentOrder = GetParentOrderID(mvt.refcon);
                if (parentOrder != 0)
                {
                    value.fCode = parentOrder;
                    list.Add(value);
                }
                else
                {
                    LOG.Write(CSMLog.eMVerbosity.M_warning, String.Format("Trade {0} does not have a parent trade!", mvt.refcon));
                }
                LOG.End();
            }
        }

        public override void GetName(int code, CMString name, long size)
        {
            name.StringValue = code.ToString();
            size = name.StringValue.Length;
        }
    }
}
