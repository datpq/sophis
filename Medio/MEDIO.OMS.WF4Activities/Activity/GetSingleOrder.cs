using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using sophis.oms;
using Sophis.Activities;
using Sophis.OMS.Activities;

namespace MEDIO.OMS.WF4Activities.Activity
{
    [Category("Medio Order Runtime")]
    [Description("Get the SingleOrder object")]
    public sealed class GetSingleOrder : CodeActivity
    {
        [Category("Medio Order Runtime")]
        [Description(@"SingleOrder")]
        public OutArgument<SingleOrder> SingleOrder
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            metadata.RequireExtension<BookmarkResumptionHelper>();
            metadata.AddDefaultExtensionProvider<BookmarkResumptionHelper>(() => new BookmarkResumptionHelper());
            RuntimeArgument arg1 = new RuntimeArgument("SingleOrder", typeof(SingleOrder), ArgumentDirection.Out);
            metadata.Bind(this.SingleOrder, arg1);
            metadata.AddArgument(arg1);
        }

        protected override void Execute(CodeActivityContext context)
        {
            IOrder order = context.GetOrder();
            SingleOrder singleOrder = (SingleOrder) order;
            if (singleOrder != null)
            {
                this.SingleOrder.Set(context, singleOrder);
            }
        }
    }
}
