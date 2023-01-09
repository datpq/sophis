using DevExpress.XtraEditors;
using sophis.instrument;
using sophis.portfolio;
using sophis.static_data;
using sophis.strategy;
using sophis.utils;
using sophis.value;
using sophis.value.benchmark;
using sophis.value.perfAnalysis;
using sophis.xaml;
using Sophis.Data.Utils;
using Sophis.PerfAnalysis.DashboardGUI;
using Sophis.PerfAnalysis.DashboardGUI.UserControls;
using sophisTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace MEDIO.OrderAutomation.NET.Source.GUI
{

    class CSxPerfDashboardMenu : CSMPositionCtxMenu
    {
        //Y=0,M=1,Q=2
        int _periodType = 0;

        protected bool _isStrategy = false;
        protected string _folioKey = "";
        protected string _stratBenchmarkName = "";
        protected int _benchId = 0;

        public CSxPerfDashboardMenu(int perType)
        {
            _periodType = perType;
            _stratBenchmarkName = "";
            _folioKey = "";
            _isStrategy = false;

        }

        public override bool IsAuthorized(ArrayList positionList)
        {
            return true;
        }

        public override int GetContextMenuGroup()
        {
            return 101;
        }

        public override bool IsFolioAuthorized(ArrayList folioList)
        {
            _isStrategy = false;
            string stratFolName = "";
            if (folioList.Count != 0)
            {
                CSMPortfolio fol = (CSMPortfolio)folioList[0];
                if (fol != null)
                {
                    int code = fol.GetCode();

                    CSMAmPortfolio strat = fol;
                    if (strat != null)
                    {
                        int stratId = strat.GetStrategy();
                        if (stratId != 0)
                        {
                            CSMAmStrategiesMgr mgr = CSMAmStrategiesMgr.GetInstance();
                            if (mgr != null)
                            {
                                CSMAmFolioStrategy strFolio = mgr.GetStrategy(stratId);
                                if (strFolio != null)
                                {
                                    int benchId = strFolio.GetBenchmark();
                                    _benchId = strFolio.GetBenchmark();
                                    CSMAmBenchmark bench = CSMAmBenchmark.GetBenchmark(benchId);
                                    if (bench != null)
                                    {
                                        _stratBenchmarkName = bench.GetName().ToString();
                                    }
                                    stratFolName = strFolio.GetName().ToString();
                                    _isStrategy = true;

                                }
                            }

                        }
                    }

                    CMString name = fol.GetName();
                    string idString = fol.GetCode().ToString();

                    if (_isStrategy == true)
                    {
                        _folioKey = stratFolName + " (Strategy)";
                    }
                    else
                    {
                        _folioKey = name + " (" + idString + ")";
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// This method is called if portfolio are selected only
        /// </summary>
        /// <param name="positionList"></param>
        /// <param name="ActionName"></param>
        public override void FolioAction(ArrayList portfolioList, CMString ActionName)
        {
            try
            {
                PerfAttributionDashboard cont = new PerfAttributionDashboard();
                XSRWinFormsAdapter<PerfAttributionDashboard> adapter = XSRWinFormsAdapter<PerfAttributionDashboard>.OpenMDIDialog(cont, "MEDIO Performance");
                DashboardParameters serDashboard = (DashboardParameters)adapter.Control.SerializableChildren[0];
                serDashboard.Width = 330;

                if (serDashboard != null)
                {
                    int folioIndex = -1;
                    foreach (var itemName in ((DevExpress.XtraEditors.ComboBoxEdit)serDashboard.Controls[11]).Properties.Items) //loop through folios dropdown
                    {
                        folioIndex++;
                        string name = itemName.ToString();
                        if (name.Equals(_folioKey) == true)
                        {
                            break;
                        }
                    }
                    ((DevExpress.XtraEditors.ComboBoxEdit)serDashboard.Controls[11]).SelectedIndex = folioIndex; //set the folio/strategy for which we are running the report


                    CMString ccyName = ((System.Windows.Forms.ComboBox)serDashboard.Controls[13].Controls.Owner).SelectedItem.ToString();
                    int ccyCode = CSMCurrency.StringToCurrency(ccyName);
                    CSMCurrency folioCcy = CSMCurrency.GetCSRCurrency(ccyCode);

                    int currentDate = sophis.market_data.CSMMarketData.GetCurrentMarketData().GetDate();
                    DateTime systemDate = DateTime.Today;
                    CSMDay sysD = new CSMDay(systemDate.Day, systemDate.Month, systemDate.Year);
                    if (currentDate == sysD.toLong())
                    {
                        currentDate = currentDate - 1;//if context date is today, we start from prev day
                    }

                    int adjustedEndDate = folioCcy.MatchingBusinessDay(currentDate, eMHolidayAdjustmentType.M_haPreceding);
                    CSMDay endD = new CSMDay(adjustedEndDate);

                    int dayS = endD.fDay;
                    int monthS = endD.fMonth;
                    int yearS = endD.fYear;

                    if (_periodType == 0)//year
                    {
                        dayS = 31;
                        monthS = 12;
                        yearS = endD.fYear - 1;
                    }
                    else if (_periodType == 1)//month
                    {
                        if (monthS == 1)
                        {
                            dayS = 31;
                            monthS = 12;
                            yearS = endD.fYear - 1;
                        }
                        else
                        {
                            DateTime firstDay = new DateTime(yearS, monthS, 1);
                            DateTime prevDay = firstDay.AddDays(-1);

                            dayS = prevDay.Day;
                            monthS = endD.fMonth - 1;
                        }

                    }
                    else if (_periodType == 2)//quarter
                    {
                        switch (monthS)
                        {
                            case 1:
                            case 2:
                            case 3:
                                {
                                    dayS = 31;
                                    monthS = 12;
                                    yearS = endD.fYear - 1;

                                }
                                break;

                            case 4:
                            case 5:
                            case 6:
                                {
                                    dayS = 31;
                                    monthS = 3;

                                }
                                break;
                            case 7:
                            case 8:
                            case 9:
                                {
                                    dayS = 30;
                                    monthS = 6;
                                }
                                break;
                            case 10:
                            case 11:
                            case 12:
                                {
                                    dayS = 30;
                                    monthS = 9;
                                }
                                break;
                        }
                    }

                    CSMDay startD = new CSMDay(dayS, monthS, yearS);
                    // int adjustedStartDate = 0;
                    // adjustedStartDate = folioCcy.MatchingBusinessDay(startD.toLong(), eMHolidayAdjustmentType.M_haPreceding);

                    DateTime sophisBase = new DateTime(1904, 01, 01, 0, 0, 0);
                    DateTime startDateAdj = sophisBase.AddDays(startD.toLong());
                    DateTime endDateAdj = sophisBase.AddDays(adjustedEndDate);

                    ((DevExpress.XtraEditors.DateEdit)serDashboard.Controls[0]).DateTime = startDateAdj;//set report start date
                    ((DevExpress.XtraEditors.DateEdit)serDashboard.Controls[1]).DateTime = endDateAdj;//set report end date

                    if (_isStrategy == true)
                    {
                        if (_stratBenchmarkName != "")
                        {
                           ((System.Windows.Forms.ComboBox)serDashboard.Controls[12]).Text = "Official (" + _stratBenchmarkName + ")";  //set strategy benchmark name             
                            ((System.Windows.Forms.ComboBox)serDashboard.Controls[12]).Focus();
                            ((DevExpress.XtraEditors.DateEdit)serDashboard.Controls[0]).Focus();
                            ((System.Windows.Forms.ComboBox)serDashboard.Controls[12]).Focus();

                        }

                    }
                }

                PerfAttribDockPanel qq = adapter.Control.GetTreePanel();
                PrefAttribDashboardTreelist testTree = (PrefAttribDashboardTreelist)((System.Windows.Forms.SplitContainer)qq.ControlContainer.Controls[0]).Panel1.Controls[0];


                if (testTree != null)
                {
                    adapter.Control.Controls[1].Width = 310;
                    ((DevExpress.XtraTreeList.TreeList)(testTree.Controls)[0]).Columns.TreeList.BestFitColumns();
                    SimpleButton btn = testTree.LoadButton;
                 
                    btn.PerformClick();
                   
                }

                adapter.ShowWindow();
            }
            catch (Exception e)
            {
                CSMLog.Write("CSxPerfDashboardMenu", "FolioAction", CSMLog.eMVerbosity.M_error, "Error:" + e.ToString());
            }

        }
    }

}
