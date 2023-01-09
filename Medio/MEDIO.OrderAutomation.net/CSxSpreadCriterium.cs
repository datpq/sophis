using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MEDIO.CORE.Tools;
using sophis.instrument;
using sophis.portfolio;
using sophis.utils;

namespace MEDIO.OrderAutomation.NET
{
    public class CSxSpreadCriterium : CSxYTMCriterium
    {
        private string fColumnName = "";

        private List<CSxBucketData> _Buckets; 

        public CSxSpreadCriterium(string columnName) : base()
        {
            fColumnName = columnName;
            _Buckets = GetYTMIntervals("Spread bucket");
        }

        public CSxSpreadCriterium()
            : base()
        {
            _Buckets = GetYTMIntervals("Spread bucket");
        }

        public override CSMCriterium Clone()
        {
            return new CSxSpreadCriterium(fColumnName);
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
                        double spread = inst.GetInstrumentSpread() * 100;
                        foreach (var one in _Buckets)
                        {
                            if (one.UnboundKind == eUnboundKind.InBetween)
                            {
                                if (spread >= one.lowEndPoint && spread <= one.highEndPoint)
                                {
                                    value.fCode = _Buckets.IndexOf(one);
                                    list.Add(value);
                                }
                            }
                            else if (one.UnboundKind == eUnboundKind.GreatThan)
                            {
                                if (spread >= one.lowEndPoint)
                                {
                                    value.fCode = _Buckets.IndexOf(one);
                                    list.Add(value);
                                }
                            }
                            else if (one.UnboundKind == eUnboundKind.LowerThan)
                            {
                                if (spread <= one.lowEndPoint)
                                {
                                    value.fCode = _Buckets.IndexOf(one);
                                    list.Add(value);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void GetName(int code, CMString name, long size)
        {
            if (code < _Buckets.Count)
            {
                name.StringValue = _Buckets[code].Name;
                size = name.StringValue.Length;
            }
        }
            
    }
}
