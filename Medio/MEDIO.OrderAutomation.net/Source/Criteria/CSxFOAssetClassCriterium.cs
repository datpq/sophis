using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using MEDIO.OrderAutomation.net.Source.Criteria;
using Oracle.DataAccess.Client;
using sophis;
using sophis.instrument;
using Sophis.OMS.Util;
using sophis.portfolio;
using sophis.utils;
using Sophis.BasicData;
using Sophis.guicommon.userControls;

namespace MEDIO.OrderAutomation.NET.Source.Criteria
{
    public class CSxFOAssetClassCriterium : CSxHedgingFundingCriterium
    {
        private readonly string fColumnName = MedioConstants.MEDIO_USERCRITERIUM_FOASSETCLASS;
        private readonly string EquityName = "Equity";

        private readonly Dictionary<int, string> _positionRankingMap = new Dictionary<int, string>();
        private readonly List<string> _rankingValues = new List<string>();

        public CSxFOAssetClassCriterium(string columnName) : base()
        {
            fColumnName = columnName;
        }

        public CSxFOAssetClassCriterium()
            : base()
        {
        }

        public override CSMCriterium Clone()
        {
            return new CSxFOAssetClassCriterium(fColumnName);
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
                value.fCode = -1;

                if (!_positionRankingMap.ContainsKey(mvt.mvtident))
                {
                    SSMCellValue posValue = CSxColumnHelper.GetPositionColumn(mvt.mvtident, mvt.folio, fColumnName);
                    _positionRankingMap[mvt.mvtident] = posValue.GetString();
                    if (!_rankingValues.Contains(posValue.GetString()))
                        _rankingValues.Add(posValue.GetString());
                }
                string str = _positionRankingMap[mvt.mvtident];
                if (IsFXDeal(mvt.sicovam))
                {
                    int parentOrder = GetParentOrderID(mvt.refcon);
                    if (parentOrder != 0)
                        str = EquityName;
                }
                if (!_rankingValues.Contains(str))
                {
                    _rankingValues.Add(str);
                }
                value.fCode = _rankingValues.IndexOf(str);
                list.Add(value);
            }
        }

        public override void GetName(int code, CMString name, long size)
        {
            if (code == -1 || _rankingValues.ElementAtOrDefault(code) == null || _rankingValues[code].IsNullOrEmpty())
            {
                name.StringValue = "N/A";
            }
            else
            {
                name.StringValue = _rankingValues[code];
                size = name.StringValue.Length;
            }
        }

    }
}
