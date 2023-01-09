
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
using sophis.value;

namespace MEDIO.OrderAutomation.net.Source.Criteria
{
    public class CSxLookthroughCriterium : CSMCriterium
    {
        private static ArrayList _LoadedFolios = new ArrayList();
        private static Dictionary<int, string> _lookMap = new Dictionary<int, string>();
        private static Dictionary<int, int> _strategyMap = new Dictionary<int, int>();
        private static List<string> _stratValues = new List<string>();

        public CSxLookthroughCriterium()
            : base()
        {

        }

        public override CSMCriterium Clone()
        {
            return new CSxLookthroughCriterium();
        }

        public override CSMCriterium.MCriterionCaps GetCaps()
        {
            return new MCriterionCaps(true, true, false);
        }

        public static void LoadMappings(int folioId)
        {
            _lookMap.Clear();
            _strategyMap.Clear();
            _stratValues.Clear();
            //ArrayList entrypoint = new ArrayList() { folioId };
            _LoadedFolios.Add(folioId);
            ArrayList criteria = new ArrayList();
            criteria.Add(CSMCriterium.GetCriteriumType("Portfolio"));
            var tktExtraction = new CSMExtractionCriteria(criteria, _LoadedFolios, true);
            tktExtraction.SetHierarchicCriteria(true);
            tktExtraction.KeepPositionId(true);
            tktExtraction.SetFilteredDeals(CSMExtraction.eMFilteredDeals.M_eNoAccess);
            tktExtraction.SetSplitByComponents(eMLookthroughType.M_eltFull);
            tktExtraction.Create();

            int nbOfFolios = tktExtraction.GetFolioCount();
            int folId = 0;

            CSMPortfolioColumn col = CSMPortfolioColumn.GetCSRPortfolioColumn("Strategy Name");
            SSMCellValue val = new SSMCellValue();
            SSMCellStyle cellStyle = new SSMCellStyle();

            for (int folItem = 0; folItem < nbOfFolios; folItem++)
            {
                folId = tktExtraction.GetNthPortfolioId(folItem);

                CSMPortfolio tktFolio = CSMPortfolio.GetCSRPortfolio(folId, tktExtraction);

                if (tktFolio != null)
                {
                    int posNb = tktFolio.GetTreeViewPositionCount();
                    for (int i = 0; i < posNb; i++)
                    {
                        CSMPosition posItem = tktFolio.GetNthTreeViewPosition(i);
                        if (posItem != null)
                        {
                            int lookCode = posItem.GetLookthroughOriginalInstrumentCode();
                            int identOrg = posItem.GetIdentifier();
                            if (identOrg > 0 && lookCode == 0)//if no lookthrough code, group by strategy
                            {
                                col.GetPositionCell(posItem, posItem.GetPortfolioCode(), posItem.GetPortfolioCode(), tktExtraction, 0, posItem.GetInstrumentCode(), ref val, cellStyle, true);
                                string stratName = val.GetString();
                                if (stratName != "")
                                {
                                    if (!_stratValues.Contains(stratName))
                                        _stratValues.Add(stratName);

                                    if (_strategyMap.ContainsKey(identOrg) == false)
                                        _strategyMap.Add(identOrg, _stratValues.IndexOf(stratName));
                                }
                            }

                            CSMInstrument fund = CSMInstrument.GetInstance(lookCode);
                            if (fund != null)
                            {
                                string lookName = fund.GetName().ToString();

                                if (_lookMap.ContainsKey(identOrg) == false)
                                    _lookMap.Add(identOrg, lookName);
                            }

                        }

                    }
                }
            }
        }


        public override void GetCode(SSMReportingTrade mvt, ArrayList list)
        {
            using (var LOG = new CSMLog())
            {
                list.Clear();
                SSMOneValue value = new SSMOneValue();
                value.fCode = -1;

                int folIdent = mvt.folio;
                CSMAmPortfolio folio = CSMAmPortfolio.GetCSRPortfolio(folIdent);
                if (folio != null)
                {
                    CSMAmPortfolio fund = folio.GetFundRootPortfolio();
                    if (fund != null)
                    {
                        folIdent = fund.GetCode();
                        if (_LoadedFolios.Contains(folIdent) == false)
                        {
                            if (folio.IsLoaded() == false)
                            {
                                folio.Load();
                            }
                            LoadMappings(folIdent);
                        }

                    }
                }

                CSMPosition pos = CSMPosition.GetCSRPosition(mvt.mvtident, mvt.folio);

                if (pos != null)
                {
                    int posId = pos.GetIdentifier();
                    if (_strategyMap.ContainsKey(posId))
                    {
                        value.fCode = _strategyMap[posId];
                    }
                    else if (_lookMap.ContainsKey(posId))
                    {
                        value.fCode = posId;
                    }
                    else
                    {
                        value.fCode = -50000;//dummy code used for OTHERS category
                    }
                }
                list.Add(value);
            }
        }


        public override void GetName(int code, CMString name, long size)
        {
            if (code == -50000)
            {
                name.StringValue = "OTHERS";
                size = name.StringValue.Length;
            }
            else if (_lookMap.ContainsKey(code))
            {
                name.StringValue = _lookMap[code];
                size = name.StringValue.Length;
            }
            else
            {
                if (_stratValues.Count > code && code >= 0)
                {
                    name.StringValue = _stratValues[code];
                    size = name.StringValue.Length;
                }
                else
                {
                    name.StringValue = "Exception";
                    size = name.StringValue.Length;
                }
            }

        }
    }
}
