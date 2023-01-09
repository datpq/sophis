using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MEDIO.CORE.Tools;
using MEDIO.MEDIO_CUSTOM_PARAM;
using sophis;
using sophis.instrument;
using sophis.portfolio;
using sophis.utils;

namespace MEDIO.OrderAutomation.NET
{
    public class CSxBucketData
    {
        public int ID;
        public string Name;
        public double lowEndPoint;
        public double highEndPoint;
        public eUnboundKind UnboundKind;
    }

    public enum eUnboundKind
    {
        LowerThan = -1,
        InBetween = 0,
        GreatThan = 1
    }

    public class CSxYTMCriterium : CSMCriterium
    {
        private string fColumnName = "";

        private List<CSxBucketData> _ytmBuckets; 

        public CSxYTMCriterium(string columnName) : base()
        {
            fColumnName = columnName;
            _ytmBuckets = GetYTMIntervals("YTM bucket");
        }

        public CSxYTMCriterium()
            : base()
        {
            _ytmBuckets = GetYTMIntervals("YTM bucket");
        }

        public override CSMCriterium Clone()
        {
            return new CSxYTMCriterium(fColumnName);
        }

        public override CSMCriterium.MCriterionCaps GetCaps()
        {
            return new MCriterionCaps(true, true, false);
        }

        public override void GetCode(SSMReportingTrade mvt, ArrayList list)
        {
            using (var LOG = new CSMLog())
            {
                list.Clear();
                SSMOneValue value = new SSMOneValue();
                CSMBond inst = CSMInstrument.GetInstance(mvt.sicovam);
                if (inst != null)
                {
                    CSMPosition pos = CSMPosition.GetCSRPosition(mvt.mvtident, mvt.folio);
                    if (pos != null)    
                    {
                        SSMCellValue column = CSxColumnHelper.GetPositionColumn(mvt.mvtident, mvt.folio,"Yield to maturity");
                        double ytm = column.doubleValue;

                        foreach (var one in _ytmBuckets)
                        {
                            if (one.UnboundKind == eUnboundKind.InBetween)
                            {
                                if (ytm >= one.lowEndPoint && ytm <= one.highEndPoint)
                                {
                                    value.fCode = _ytmBuckets.IndexOf(one);
                                    list.Add(value);
                                }
                            }
                            else if (one.UnboundKind == eUnboundKind.GreatThan)
                            {
                                if (ytm >= one.lowEndPoint)
                                {
                                    value.fCode = _ytmBuckets.IndexOf(one);
                                    list.Add(value);
                                }
                            }
                            else if (one.UnboundKind == eUnboundKind.LowerThan)
                            {
                                if (ytm <= one.lowEndPoint)
                                {
                                    value.fCode = _ytmBuckets.IndexOf(one);
                                    list.Add(value);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void GetCode(CSMPosition position, ArrayList list)
        {
           
        }

        public override void GetName(int code, CMString name, long size)
        {
            if (code < _ytmBuckets.Count)
            {
                name.StringValue = _ytmBuckets[code].Name;
                size = name.StringValue.Length;
            }
        }

        protected List<CSxBucketData> GetYTMIntervals(string bucketName)
        {
            using (var LOG = new CSMLog())
            {
                List<CSxBucketData> res = new List<CSxBucketData>();
                LOG.Begin("CSxYTMCriterium", "GetYTMIntervals");

                string sql = String.Format("select id, name from buckets where BUCKET_SET_ID = (select id from bucket_sets where name = '{0}')", bucketName);
                var idNames = CSxDBHelper.GetMultiRecords<int, string>(sql);
                foreach (var keyValuePair in idNames)
                {
                    CSxBucketData toAdd = new CSxBucketData();
                    toAdd.ID = keyValuePair.Key;
                    toAdd.Name = keyValuePair.Value;

                    sql = String.Format("select low_end_point, high_end_point,UNBOUND_KIND from bucket_intervals where BUCKET_ID = {0}", keyValuePair.Key);
                    var values = CSxDBHelper.GeTupleList<double, double, int>(sql);

                    foreach (var value in values)
                    {
                        if (value.Item3 == 0)
                        {
                            toAdd.UnboundKind = eUnboundKind.InBetween;
                            if (String.IsNullOrEmpty(toAdd.Name))
                            {
                                toAdd.Name = value.Item1 + "-" + value.Item2;
                            }
                        }
                        else if (value.Item3 == 1)
                        {
                            toAdd.UnboundKind = eUnboundKind.GreatThan;
                            if (String.IsNullOrEmpty(toAdd.Name))
                            {
                                toAdd.Name = ">=" + value.Item1;
                            }
                        }
                        else if (value.Item3 == -1)
                        {
                            toAdd.UnboundKind = eUnboundKind.LowerThan;
                            if (String.IsNullOrEmpty(toAdd.Name))
                            {
                                toAdd.Name = "<=" + value.Item1;
                            }
                        }
                        toAdd.lowEndPoint = value.Item1;
                        toAdd.highEndPoint = value.Item2;
                        res.Add(toAdd);
                    }
                }
                return res;
            }
        }

    }
}
