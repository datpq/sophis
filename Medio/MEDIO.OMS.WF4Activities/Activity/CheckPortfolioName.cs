using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using MEDIO.MEDIO_CUSTOM_PARAM;
using sophis;
using sophis.instrument;
using sophis.oms;
using sophis.portfolio;
using sophis.static_data;
using sophis.utils;
using Sophis.Activities;
using Sophis.Logging;
using Sophis.OMS;
using Sophis.OMS.Activities;
using Sophis.WF.Core;

namespace MEDIO.OMS.WF4Activities.Activity
{
    public sealed class CheckPortfolioName : CodeActivity
    {
        private static readonly ILogger _logger = LogManager.Instance.CreateCurrentClassLogger();

        [Category("Medio order routing")]
        [Description(@"True if all allocations contain the folio name, by default 'MAML'")]
        public OutArgument<bool> IsValidportfolioName
        {
            get;
            set;
        }
        
        public string PortfolioName { get; set; }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            metadata.RequireExtension<BookmarkResumptionHelper>();
            metadata.AddDefaultExtensionProvider<BookmarkResumptionHelper>(() => new BookmarkResumptionHelper());
            RuntimeArgument arg1 = new RuntimeArgument("IsValidportfolioName", typeof(bool), ArgumentDirection.Out);
            metadata.Bind(this.IsValidportfolioName, arg1);
            metadata.AddArgument(arg1);
            PortfolioName = CSxToolkitCustomParameter.Instance.OMS_WF_PORTFOLIONAME;
        }

        protected override void Execute(CodeActivityContext context)
        {
            
            _logger.LogDebug("CheckPortfolioName::Execute=BEGIN");
            IOrder order = context.GetOrder();

            SingleOrder singleOrder = (SingleOrder)order;
            if (singleOrder != null)
            {
                this.IsValidportfolioName.Set(context, IsFolioName(singleOrder));
            }
            else 
            {
                IOrder groupOrder = null;
                IGroupable groupable = order as IGroupable;
                if (groupable != null)
                {
                    if (groupable.GroupID.HasValue && groupable.GroupID.Value > 0)
                    {
                        try
                        {
                            groupOrder = OrderManager.Instance.GetOrderById(groupable.GroupID.Value);
                        }
                        catch (NotFoundOrderException ex)
                        {
                            _logger.LogError(ex.ToString());
                        }
                    }
                }
                if (groupOrder != null)
                {
                    OrderGroup gOrder = groupOrder as OrderGroup;
                    if (gOrder != null)
                    {
                        foreach (var sOrder in gOrder.Orders)
                        {
                            this.IsValidportfolioName.Set(context, IsFolioName(sOrder));
                        }
                    }
                }
            }
            
        }

        private bool IsFolioName(SingleOrder singleOrder)
        {
            
            _logger.LogDebug("CheckPortfolioName::IsFolioName=BEGIN");
            bool containsName = true;
            foreach (var allocation in singleOrder.AllocationRulesSet.Allocations)
            {
                var folioId = allocation.PortfolioID;
                SophisWcfConfig.SynchronizationContext.Send(delegate(object _)
                {
                    CSMPortfolio folio = CSMPortfolio.GetCSRPortfolio(folioId);
                    if (folio != null)
                    {
                        CMString fullName = new CMString();
                        folio.GetName(fullName);
                        containsName = fullName.StringValue.Contains(PortfolioName);
                        _logger.LogDebug("Order {0} allocation contains folio name? {1}. Folio = {2}", singleOrder.ID, containsName, fullName.StringValue);
                    }
                }, null);
            }
            return containsName;
        }
        
    }
}
