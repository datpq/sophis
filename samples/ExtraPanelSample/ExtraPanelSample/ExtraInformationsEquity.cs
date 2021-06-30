using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using sophis.oms.entry;
using sophis.oms;
using System.Linq;


namespace ExtraPanelSample
{
    public partial class ExtraInformationsEquity : DevExpress.XtraEditors.XtraUserControl, IEntryBoxExtraPanel
    {
        public ExtraInformationsEquity()
        {
            InitializeComponent();
        }

        #region IEntryBoxExtraPanel Members

        public string GetName()
        {
            return "Informations for Equities";
        }

        public void Initialize(IOrder order)
        {
            InitFolio(order);
        }

        public void OnOrderPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            IOrder order = (sender as IOrder);

            switch (args.PropertyName)
            {
                case "Quantity":
                    {
                        TxtQuantity.Text = order.QuantityData.OrderedQty.ToString();
                        break;
                    }
                case "TradeAllocation":
                    {
                        InitFolio(order);
                        break;
                    }
                case "ExternalSystem":
                    {
                        break;
                    }
                case "Side":
                    {
                        break;
                    }
                case "ThirdParties":
                    {
                        break;
                    }
            }
        }

        #endregion

        #region Tools
        void InitFolio(IOrder order)
        {
            var targetOwners = order.GetOrderTargetOwners();

            if (targetOwners.Any() && targetOwners.FirstOrDefault().AllocationRulesSet != null && targetOwners.FirstOrDefault().AllocationRulesSet.Allocations.Any())
            {
                TxtFolio.Text = string.Join(";", targetOwners.FirstOrDefault().AllocationRulesSet.Allocations.Select(a => a.PortfolioID));
            }
        }
        #endregion
    }
}
