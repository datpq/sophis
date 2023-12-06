using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MEDIO.OrderAutomation.net.Source.Tools;
using MEDIO.OrderAutomation.NET.Source.DataModel;
using sophis.guicommon.basicDialogs;
using MEDIO.CORE.Tools;
using sophis.portfolio;
using sophis.utils;
using Sophis.Windows.Integration;
using sophis.TimeMachine.Setup;
using sophis.market_data;
using sophis.oms;
using Sophis.OrderBookCompliance;
using sophis.static_data;
using System.Drawing;
using DevExpress.XtraTreeList.Nodes;

namespace MEDIO.OrderAutomation.NET.Source.GUI
{
    public partial class CSxFXAutomationForm : DefaultEmbeddableControl
    {
        public const string SectionName = "FXAutomation";

        public CSxFXAutomationForm()
        {
            InitializeComponent();
            currencyCbo.Items.AddRange(FxAutoCurrency.GetCurrencies().ToArray());
            RefreshButtons();
        }

        public static void Display()
        {
            var frm = new CSxFXAutomationForm();
            //var adapter = XSAMWinFormsAdapter<CSxFXAutomationForm>.OpenAmMDIDialog(
            //    frm, "FX Automation", null, 8000, null, null, true);
            var adapter = sophis.xaml.XSRWinFormsAdapter<CSxFXAutomationForm>.GetUniqueDialog(
                new WindowKey(12977, 0, 0), frm.Title, () => frm, true);
            if (adapter == null) return;
            adapter.ShowWindow();
        }

        private void RefreshAmountByCurrency()
        {
            var arrData = treeDCB.DataSource as ICollection<FxAutoCDB>;
            if (arrData == null) return;
            arrData.Where(x => x.NodeType == 1).ToList().ForEach(x =>//Sleeve level
            {
                x.MedioMarketValueCurGlb = x.MedioMarketValue * net.Source.Tools.CSxUtils.GetExchangeRate(x.Currency, x.CurrencyOri, dateParam.DateTime);
                if (x.BPS != 0) x.Threshold = x.MedioMarketValueCurGlb * 0.0001 * x.BPS;
            });
            arrData.Where(x => x.NodeType >= 3).ToList().ForEach(x =>//SettlementDate level && Position level
            {
                var sleeveNode = arrData.Single(y => y.ID == x.ParentID.Split('_')[0] && y.NodeType == 1);
                x.AmountCurGlb = x.AmountRounded.HasValue ? x.AmountRounded * net.Source.Tools.CSxUtils.GetExchangeRate(sleeveNode.Currency, x.Currency, dateParam.DateTime) : null;
            });
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            cmdRaiseOrders.Enabled = false;
            int cnt = 0;
            foreach (var node in treeDCB.GetNodeList())
            {
                if (node.CheckState == CheckState.Unchecked) continue;
                if (canRaiseOrder(node)) cnt++;
            }
            cmdRaiseOrders.Enabled = cnt > 0;
            cmdRaiseOrders.Text = cnt == 0 ? "Raise Order" : cnt == 1 ? "Raise 1 Order" : $"Raise {cnt} Orders";
        }

        private void cmdGenDCB_Click(object sender, EventArgs e)
        {
            using (var log = new CSMLog())
            {
                log.Begin("CSxDBHelper", MethodBase.GetCurrentMethod().Name);
                //if (treeSleeves.Selection.Count == 0)
                if (GetCheckedSleeves().Count() == 0)
                {
                    MessageBox.Show("Please select some sleeves!", Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var lstSleeveIds = GetCheckedSleeves().Select(x => x.ID).ToArray();
                var markets = cboMarkets.Properties.GetItems().GetCheckedValues().OfType<string>().ToList();
                if (markets.Count == 0)
                {
                    MessageBox.Show("Please select some markets!", Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var lstCurrencies = new List<string>();
                markets.ForEach(x =>
                {
                    FxAutoCurrency.GetData(x, true).ToList().ForEach(y => {
                        if (!lstCurrencies.Contains(y.Code)) lstCurrencies.Add(y.Code);
                    });
                });

                Cursor.Current = Cursors.WaitCursor;
                var arrData = FxAutoCDB.FetchData(dateParam.DateTime, lstSleeveIds, lstCurrencies.ToArray());
                Cursor.Current = Cursors.Default;
                //Activate the time machine when date is in the pass
                if (dateParam.DateTime < DateTime.Today)
                {
                    sophisTools.CSMDay csmDate = new sophisTools.CSMDay(dateParam.DateTime.Day, dateParam.DateTime.Month, dateParam.DateTime.Year);
                    CSMMarketData.SSMDates date = new CSMMarketData.SSMDates();
                    date.SetDatesOnly(csmDate.toLong());
                    CSMTimeMachineSetup timeMachineSetup = new CSMTimeMachineSetup();
                    timeMachineSetup.ActiveGlobalTimeContextByDate(date);
                }
                arrData.Where(x => x.NodeType == 1).ToList().ForEach(x =>//Sleeve level
                {
                    x.CurrencyOri = $@"
SELECT UPPER(DEVISE_TO_STR(DEVISECTT))
FROM TITRES T JOIN FOLIO F ON T.SICOVAM = F.SICOVAM
WHERE F.IDENT = {x.ID}".ExecuteScalar<string>();
                    var folio = CSMPortfolio.GetCSRPortfolio(int.Parse(x.ID));
                    if (!folio.IsLoaded()) folio.Load();
                    x.MedioMarketValue = CSxColumnHelper.GetPortfolioColumn(int.Parse(x.ID), null, "Medio Market Value curr. global").doubleValue;
                });
                treeDCB.DataSource = arrData;
                RefreshAmountByCurrency();
                foreach (var dcb in arrData)
                {
                    if (dcb.NodeType <= 1) continue;
                    var sleeveNode = arrData.Single(y => y.ID == dcb.ParentID.Split('_')[0] && y.NodeType == 1);
                    dcb.WeightNav = dcb.AmountCurGlb.HasValue ? (double?)Math.Abs(dcb.AmountCurGlb.Value * 100 / sleeveNode.MedioMarketValueCurGlb.Value) : null;
                    //var rbcStrategyNav = CSxColumnHelper.GetPortfolioColumn(int.Parse(sleeveNode.ID), null, "RBC Strategy NAV");
                    //log.Write(CSMLog.eMVerbosity.M_info, $"SleeveId={sleeveNode.ID}, RBC Strategy NAV={rbcStrategyNav.doubleValue}");
                    //dcb.WeightNav = dcb.AmountRounded.HasValue ? (double?)Math.Abs(dcb.AmountRounded.Value * 100 / rbcStrategyNav.doubleValue) : null;
                }
                RefreshRaisedOrders();
                treeDCB.ExpandAll();
            }
        }

        private void cmdRaiseOrders_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Do you want to {cmdRaiseOrders.Text.ToLower()}?",
                Title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) return;
            var arrData = treeDCB.DataSource as ICollection<FxAutoCDB>;
            if (arrData == null) return;
            using (var log = new CSMLog())
            {
                log.Begin("CSxFXAutomationForm", MethodBase.GetCurrentMethod().Name);
                int cnt = 0;
                foreach (var node in treeDCB.GetNodeList())
                {
                    if (node.CheckState == CheckState.Unchecked) continue;
                    if (!canRaiseOrder(node)) continue;
                    var dcbNode = treeDCB.GetDataRecordByNode(node) as FxAutoCDB;
                    var sleeveNode = arrData.Single(y => y.ID == dcbNode.ParentID.Split('_')[0] && y.NodeType == 1);
                    try
                    {
                        var res = new SingleOrder();
                        res.SettlementDate = dcbNode.DateSettlement.Value;
                        res.Side = dcbNode.AmountRounded > 0 ? ESide.Buy : ESide.Sell;
                        res.ExternalSystem = "FXALL";
                        var forexSpot = CSAMInstrumentDotNetTools.GetForexSpotMarketWay(CSMCurrency.StringToCurrency(dcbNode.Currency),
                            CSMCurrency.StringToCurrency(sleeveNode.Currency));
                        if (forexSpot != null)
                        {
                            var target = new ForexTarget();
                            target.Expiry = res.SettlementDate;
                            target.SecurityType = ESecurityType.Forex;
                            target.SecurityID = forexSpot.GetCode();
                            target.Currency = CSMCurrency.StringToCurrency(dcbNode.Currency);
                            target.Allotment = forexSpot.GetAllotment();
                            target.Market = forexSpot.GetMarketCode();
                            res.Target = target;
                        }
                        var alloc = new AllocationRule();
                        alloc.PortfolioID = int.Parse(sleeveNode.ID);
                        alloc.Quantity = Math.Round(dcbNode.AmountRounded.Value, 2);
                        alloc.EntityID = (int)$@"SELECT CONNECT_BY_ROOT ENTITE ENTITE FROM FOLIO
WHERE IDENT = {sleeveNode.ID} AND LEVEL = 3 CONNECT BY PRIOR IDENT = MGR".ExecuteScalar<decimal>();
                        res.AllocationRulesSet.Allocations.Add(alloc);
                        //res.AllocationRulesSet.QuantityType = EQuantityType.Amount;

                        res.CreationInfo = new CreationInfo();
                        res.CreationInfo.TimeStamp = DateTime.Now;
                        // Set current user
                        CSMUserRights user = new CSMUserRights();
                        res.CreationInfo.UserID = user.GetIdent();
                        res.CreationInfo.ProfileID = user.GetAllProfiles().Cast<int>().ToList().FirstOrDefault();
                        res.CreationInfo.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                        res.CreationInfo.WorkStation = Environment.MachineName;
                        res.Assignation = new Assignation();
                        res.Assignation.TimeStamp = DateTime.Now;
                        res.Assignation.UserID = user.GetIdent();
                        res.Assignation.ProfileID = res.CreationInfo.ProfileID;
                        res.Assignation.WindowsIdentity = res.CreationInfo.WindowsIdentity;
                        res.Assignation.WorkStation = res.CreationInfo.WorkStation;

                        res.QuantityData = new Quantity();
                        //res.QuantityData.ExecutedAmount = 0;
                        //res.QuantityData.ExecutedQty = 0;
                        //res.QuantityData.ExecutionsCount = 0;
                        //res.QuantityData.IsTotallyExecuted = false;
                        //res.QuantityData.AllocatedQty = 0;
                        res.QuantityData.OrderedQty = Math.Round(Math.Abs(dcbNode.AmountRounded.Value), 2);

                        res.OriginationStrategy = 16;//FX Automation
                        res.EffectiveTime = DateTime.Now;
                        res.BusinessEventId = 1;//P/S
                        res.TimeInForce = new TimeInForce { Type = ETimeInForce.GoodTillCancel, ExpiryDate = DateTime.Today };
                        string commentSicovams = string.Join(",", node.Nodes.Where(x => (treeDCB.GetDataRecordByNode(x) as FxAutoCDB).CanRaise && x.Checked).Select(x => (treeDCB.GetDataRecordByNode(x) as FxAutoCDB).ID.Split('_')[3]));
                        res.Comments.Add(new CommentInfo { Value = $"{user.GetName()}: Order generated from Medio FX Automation ({commentSicovams})" });
                        res.SettlementType = ESettlementType.Regular;

                        var newOrders = OrderManagerConnector.Instance.GetOrderManager().CreateOrders(new List<IOrder> { res }, false, res.CreationInfo).ToList();
                        newOrders.ForEach(x => OrderCreationValidatorManager.Instance.Validate(x, true));
                        newOrders = OrderManagerConnector.Instance.GetOrderManager().CreateOrders(newOrders, true, res.CreationInfo).ToList();
                        cnt++;
                        //var sendingReport = new SimulatedOMSOrdersSendingReport("FXAutomation");
                        //var sm = sophis.OrderGeneration.FxSwapManagerForDOB.new_FxSwapManager(sendingReport);
                        //OESingleOrder oeSingleOrder = OEOrderFactory.GetHandler(res, res.Kind) as OESingleOrder;
                        //if (oeSingleOrder != null)
                        //{
                        //    oeSingleOrder.InitializeTargetInstrument(oeSingleOrder.GetInstrumentCode());
                        //}
                        //sm.AddFxOrder(oeSingleOrder);
                    }
                    catch (Exception ex)
                    {
                        log.Write(CSMLog.eMVerbosity.M_error, ex.Message);
                        MessageBox.Show(ex.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        throw ex;
                    }
                }
                MessageBox.Show($"{cnt} Order(s) raised successfully", Title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshRaisedOrders();
            }
        }

        private void CSxFXAutomationForm_Load(object sender, EventArgs e)
        {
            dateParam.DateTime = DateTime.Today;
            cboMarkets.Properties.DataSource = FxAutoMarket.GetData(true);
        }

        private void treeSleeves_GetStateImage(object sender, DevExpress.XtraTreeList.GetStateImageEventArgs e)
        {
            e.NodeImageIndex = e.Node.HasChildren ? 0 : 1;
        }

        private IEnumerable<FXAutoSleeveFolio> GetCheckedSleeves()
        {
            return treeSleeves.GetAllCheckedNodes().Where(x => !x.HasChildren).Select(
                x => (FXAutoSleeveFolio)treeSleeves.GetDataRecordByNode(x));
        }

        private void treeSleeves_AfterCheckNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            lookupSleeves.EditValue = string.Join(",", GetCheckedSleeves().Select(x => x.Name));
        }

        private void lookupSleeves_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e)
        {
            e.DisplayText = string.Join(",", GetCheckedSleeves().Select(x => x.Name));
        }

        private void treeDCB_GetStateImage(object sender, DevExpress.XtraTreeList.GetStateImageEventArgs e)
        {
            var node = treeDCB.GetDataRecordByNode(e.Node) as FxAutoCDB;
            e.NodeImageIndex = node.NodeType;
        }

        private void lookupSleeves_Properties_BeforePopup(object sender, EventArgs e)
        {
            //lookupSleeves.Properties.DataSource = FXAutoSleeveFolio.GetData(true);
            if (needReloadSleeves)
            {
                cmdRefreshSleeves_Click(null, null);
            }
        }

        private bool needReloadSleeves = false;
        private void dateParam_Properties_DateTimeChanged(object sender, EventArgs e)
        {
            needReloadSleeves = true;
        }

        private void treeDCB_ShowingEditor(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (treeDCB.FocusedColumn == colCurrency || treeDCB.FocusedColumn == colBPS || treeDCB.FocusedColumn == colThreshold)
            {
                e.Cancel = true;
                var node = treeDCB.GetDataRecordByNode(treeDCB.FocusedNode) as FxAutoCDB;
                if (node == null || node.NodeType != 1) return;
                if (node.BPS != 0 && treeDCB.FocusedColumn == colThreshold) return;//BPS != 0, cannot change the threshold
                e.Cancel = false;//Only editable in Sleeve level
            }
        }

        private void treeDCB_CellValueChanged(object sender, DevExpress.XtraTreeList.CellValueChangedEventArgs e)
        {
            var dcbNode = treeDCB.GetDataRecordByNode(treeDCB.FocusedNode) as FxAutoCDB;
            if (dcbNode == null || dcbNode.NodeType != 1) return;
            if (treeDCB.FocusedColumn == colCurrency)
            {
                var rowCount = net.Source.Tools.CSxUtils.ExecuteNonQuery($"UPDATE MEDIO_FXAUTO_SLEEVES SET CCY = '{dcbNode.Currency}' WHERE IDENT = {dcbNode.ID}");
                if (rowCount == 0) net.Source.Tools.CSxUtils.ExecuteNonQuery($"INSERT INTO MEDIO_FXAUTO_SLEEVES(IDENT, CCY) VALUES({dcbNode.ID}, '{dcbNode.Currency}')");
                RefreshAmountByCurrency();
            } else if (treeDCB.FocusedColumn == colBPS)
            {
                var rowCount = net.Source.Tools.CSxUtils.ExecuteNonQuery($"UPDATE MEDIO_FXAUTO_SLEEVES SET BPS = {dcbNode.BPS} WHERE IDENT = {dcbNode.ID}");
                if (rowCount == 0) net.Source.Tools.CSxUtils.ExecuteNonQuery($"INSERT INTO MEDIO_FXAUTO_SLEEVES(IDENT, BPS) VALUES({dcbNode.ID}, {dcbNode.BPS})");
                if (dcbNode.BPS != 0) net.Source.Tools.CSxUtils.ExecuteNonQuery($"UPDATE MEDIO_FXAUTO_SLEEVES SET THRESHOLD = NULL WHERE IDENT = {dcbNode.ID}");
                RefreshAmountByCurrency();
            } else if (treeDCB.FocusedColumn == colThreshold)
            {
                string thresholdSQL = dcbNode.Threshold.HasValue ? dcbNode.Threshold.ToString() : "NULL";
                var rowCount = net.Source.Tools.CSxUtils.ExecuteNonQuery($"UPDATE MEDIO_FXAUTO_SLEEVES SET THRESHOLD = {thresholdSQL} WHERE IDENT = {dcbNode.ID}");
                if (rowCount == 0) net.Source.Tools.CSxUtils.ExecuteNonQuery($"INSERT INTO MEDIO_FXAUTO_SLEEVES(IDENT, THRESHOLD) VALUES({dcbNode.ID}, {thresholdSQL})");
            }
            RefreshButtons();
        }

        private void cmdCopy_Click(object sender, EventArgs e)
        {
            treeDCB.CopyToClipboard();
        }

        private void RefreshSettlementDateAmounts()
        {
            foreach (var node in treeDCB.GetNodeList())
            {
                var dcbNode = treeDCB.GetDataRecordByNode(node) as FxAutoCDB;
                if (dcbNode == null || dcbNode.NodeType != 3) continue;//SettlementDate level only

                double amount = 0, amountCurGlb = 0;
                double amountAll = 0, amountCurGlbAll = 0;
                foreach (TreeListNode child in node.Nodes)
                {
                    var childDcbNode = treeDCB.GetDataRecordByNode(child) as FxAutoCDB;
                    amountAll += childDcbNode.AmountRounded.Value;
                    amountCurGlbAll += childDcbNode.AmountCurGlb.Value;
                    if (!childDcbNode.CanRaise || !child.Checked) continue;
                    amount += childDcbNode.AmountRounded.Value;
                    amountCurGlb += childDcbNode.AmountCurGlb.Value;
                }
                dcbNode.AmountRounded = node.CheckState == CheckState.Unchecked ? amountAll : amount;
                dcbNode.AmountCurGlb = node.CheckState == CheckState.Unchecked ? amountCurGlbAll : amountCurGlb;
                treeDCB.RefreshNode(node);
            }
        }

        private void treeDCB_AfterCheckNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            RefreshSettlementDateAmounts();
            RefreshButtons();
        }

        private bool canRaiseOrder(TreeListNode node)
        {
            var dcbNode = treeDCB.GetDataRecordByNode(node) as FxAutoCDB;
            if (dcbNode == null || dcbNode.NodeType != 3) return false;//SettlementDate level only
            var arrData = treeDCB.DataSource as ICollection<FxAutoCDB>;
            if (arrData == null) return false;
            var sleeveNode = arrData.Single(y => y.ID == dcbNode.ParentID.Split('_')[0] && y.NodeType == 1);

            if (node.CheckState == CheckState.Unchecked)
            {
                return sleeveNode.Currency != dcbNode.Currency && sleeveNode.Threshold.HasValue
                    && Math.Abs(dcbNode.AmountCurGlb.Value) > sleeveNode.Threshold;
            }
            double amountCurGlb = 0;
            foreach (TreeListNode child in node.Nodes)
            {
                if (!child.Checked) continue;
                var childNode = treeDCB.GetDataRecordByNode(child) as FxAutoCDB;
                if (childNode.CanRaise) amountCurGlb += childNode.AmountCurGlb.Value;
            }
            bool ans = sleeveNode.Currency != dcbNode.Currency && sleeveNode.Threshold.HasValue
                && Math.Abs(amountCurGlb) > sleeveNode.Threshold;
            return ans;
        }

        private void treeDCB_NodeCellStyle(object sender, DevExpress.XtraTreeList.GetCustomNodeCellStyleEventArgs e)
        {
            bool raisable = canRaiseOrder(e.Node);
            e.Appearance.BackColor = raisable ? Color.Lavender : Color.Empty;
            e.Appearance.FontStyleDelta = raisable && e.Node.CheckState != CheckState.Unchecked ? FontStyle.Bold : FontStyle.Regular;
            var dcbNode = treeDCB.GetDataRecordByNode(e.Node) as FxAutoCDB;
            if (dcbNode.NodeType == 4) {
                e.Appearance.BackColor = dcbNode.CanRaise ? Color.Empty : Color.LightGray;
            }
        }

        private void RefreshRaisedOrders()
        {
            var arrData = treeDCB.DataSource as ICollection<FxAutoCDB>;
            if (arrData == null) return;
            arrData.Where(x => x.NodeType == 3).ToList().ForEach(x =>//SettlementDate level
            {
                var sleeveNode = arrData.Single(y => y.ID == x.ParentID.Split('_')[0] && y.NodeType == 1);
                var settlementDateStr = x.DateSettlement.Value.ToString("yyyyMMdd");
                x.AmountRaised = (double?)$@"
SELECT SUM(ABS(OA.QUANTITY)) FROM ORDER_DESCRIPTOR OD
    JOIN ORDER_ALLOCATION OA ON OA.ORDERID = OD.ID AND OA.PORTFOLIOID = {sleeveNode.ID}
    JOIN ORDER_WELLKNOWNTARGET OW ON OW.ID = OD.TARGETID
    JOIN TITRES T ON T.SICOVAM = OW.SECURITYID AND T.DEVISECTT = STR_TO_DEVISE('{x.Currency}') AND T.MARCHE = STR_TO_DEVISE('{sleeveNode.Currency}')
WHERE OD.ORIGINATIONID = 16 AND OD.ISALIVE = 1
    AND OD.SETTLEMENTDATE = TO_DATE('{settlementDateStr}', 'YYYYMMDD')".ExecuteScalar<decimal?>();
            });
            arrData.Where(x => x.NodeType == 4).ToList().ForEach(x =>//Instrument level
            {
                var sleeveNode = arrData.Single(y => y.ID == x.ParentID.Split('_')[0] && y.NodeType == 1);
                var sicovam = x.ID.Split('_')[3];
                var settlementDateStr = x.DateSettlement.Value.ToString("yyyyMMdd");
                var orderId = (int?)$@"
SELECT OD.ID FROM ORDER_DESCRIPTOR OD
    JOIN ORDER_ALLOCATION OA ON OA.ORDERID = OD.ID AND OA.PORTFOLIOID = {sleeveNode.ID}
    JOIN ORDER_WELLKNOWNTARGET OW ON OW.ID = OD.TARGETID
    JOIN TITRES T ON T.SICOVAM = OW.SECURITYID AND T.DEVISECTT = STR_TO_DEVISE('{x.Currency}') AND T.MARCHE = STR_TO_DEVISE('{sleeveNode.Currency}')
    JOIN ORDER_COMMENT OC ON OC.ORDERID = OD.ID AND OC.COMMENT_VALUE LIKE '%{sicovam}%'
WHERE OD.ORIGINATIONID = 16 AND OD.ISALIVE = 1
    AND OD.SETTLEMENTDATE = TO_DATE('{settlementDateStr}', 'YYYYMMDD')".ExecuteScalar<decimal?>();
                x.CanRaise = orderId == null;
            });
            treeDCB.UncheckAll();
            treeDCB.RefreshDataSource();
            RefreshButtons();
        }

        private void cmdRefreshSleeves_Click(object sender, EventArgs e)
        {
            lookupSleeves.Properties.DataSource = FXAutoSleeveFolio.GetDataByTradeDate(dateParam.DateTime);
            needReloadSleeves = false;
        }

        private void treeDCB_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            if (treeDCB.FocusedColumn == colBPS)
            {
                if (!int.TryParse(e.Value.ToString(), out int val))
                {
                    e.Value = 0;
                    e.Valid = true;
                    e.ErrorText = string.Empty;
                }
            } else if (treeDCB.FocusedColumn == colThreshold)
            {
                if (!double.TryParse(e.Value.ToString(), out double val))
                {
                    e.Value = null;
                    e.Valid = true;
                    e.ErrorText = string.Empty;
                }
            }
        }
    }
}
