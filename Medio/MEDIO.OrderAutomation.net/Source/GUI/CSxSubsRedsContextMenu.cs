using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.XtraEditors;
using sophis.instrument;
using sophis.misc;
using sophis.portfolio;
using sophis.static_data;
using sophis.strategy;
using sophis.utils;
using sophis.value;
using sophis.xaml;
using Sophis.Data.Utils;
using Sophis.Util.GUI;

namespace MEDIO.OrderAutomation.NET.Source.GUI
    {
        class CSxSubsRedsContextMenu : CSMPositionCtxMenu
        {
            public CSxSubsRedsContextMenu()
            {

            }

            public override bool IsAuthorized(ArrayList positionList)
            {
                return false;
            }

            public override int GetContextMenuGroup()
            {
                return 101;
            }

            public override bool IsFolioAuthorized(ArrayList folioList)
            {
                bool result = false;
             
            if (folioList.Count != 0)
            {
                
                foreach(var folCode in folioList)
                {
                    CSMPortfolio fol = (CSMPortfolio)folCode;
                    if (fol != null)
                    {
                        int code = fol.GetCode();
                        using (CSMAmPortfolio fundFolio = CSMAmPortfolio.GetCSRPortfolio(code,fol.GetExtraction()))
                        {
                            if (fundFolio != null)
                            {
                                using (CSMAmPortfolio fundRoot = fundFolio.GetFundRootPortfolio())
                                {
                                    if (fundRoot != null)
                                    {
                                        if (fundRoot.IsLoaded())
                                        {
                                            result = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    }
                }
                return result;
            }

            /// <summary>
            /// This method is called if portfolio are selected only
            /// </summary>
            /// <param name="positionList"></param>
            /// <param name="ActionName"></param>
            public override void FolioAction(ArrayList portfolioList, CMString ActionName)
            {
            ArrayList folioCodes = new ArrayList();
            foreach (CSMPortfolio itm in portfolioList)
            {
                using (CSMAmPortfolio fundFolio = CSMAmPortfolio.GetCSRPortfolio(itm.GetCode(), itm.GetExtraction()))
                {
                    if (fundFolio != null)
                    {
                        ArrayList foliosIdent = new ArrayList();
                        fundFolio.GetRealPortfoliosFromFundAndStrategy(foliosIdent);

                        foreach (int folId in foliosIdent)
                        {
                            using (CSMAmPortfolio realFolio = CSMAmPortfolio.GetCSRPortfolio(folId))
                                if (realFolio != null)
                                {
                                    using (CSMAmPortfolio fundRoot = realFolio.GetFundRootPortfolio())
                                    {
                                        if (realFolio != null && fundRoot.GetCode() == folId)
                                        {
                                            
                                            string mainStrategy = CSxSummaryController.GetMIFLMainStrat(folId);
                                            if (CSxSummaryController._MIFLStrategiesMap.ContainsKey(folId) == false && mainStrategy != "")
                                            {
                                                CSxSummaryController._MIFLStrategiesMap.Add(folId, mainStrategy);
                                            }
                                            folioCodes.Add(folId);
                                        }
                                    }
                                }
                            
                        }
                    }
                }
            }
            CSxSummaryScreen cashScreen = new CSxSummaryScreen(folioCodes);
            XSRWinFormsAdapter<CSxSummaryScreen> adapter = XSRWinFormsAdapter<CSxSummaryScreen>.OpenMDIDialog(cashScreen, "Subs & Reds Summary");
             if (!adapter.IsVisible)
                adapter.ShowWindow();

        }
        }

    }


