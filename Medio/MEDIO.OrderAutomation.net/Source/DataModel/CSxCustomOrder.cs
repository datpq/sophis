using sophis.oms;
using sophis.oms.execution;

namespace MEDIO.OrderAutomation.net.Source.DataModel
{
    public class CSxAllocationRule : AllocationRule
    {
        public string PortfolioPath { get; set; }
        public string PortfolioName { get; set; }
        public double FundingProportion { get; set; }
        public double HedgingProportion { get; set; }
        public double Proportion { get; set; }
        public string FundCurrency { get; set; }
        /// <summary>
        ///  %NAV - Funding : ([Funding%*executed amount ]/NAV)*100%
        /// </summary>
        public double NAVFunding { get; set; }
        /// <summary>
        ///  %NAV – Hedging : ([Hedging%*executed amount]/NAV )*100%
        /// </summary>
        public double NAVHedging { get; set; }
        public double InitQty { get; set; }
        public double InitPercent { get; set; }
        public double ExecQty { get; set; }
        public double ExecAmount { get; set; }
        public double ExecPercent { get; set; }
    }

    public class CSxOrderExecution : OrderExecution
    {
        public virtual double ExecutedAmount { get; set; }
        public virtual double ExecutedQuantity { get; set; }
        public virtual string ExternalId { get; set; }
        public virtual string ExternalSystem { get; set; }
        public virtual int Id { get; set; }
        public string Side { get; set; }
        public string PaymentCCY { get; set; }
        public double Price { get; set; }
    }

    public class CSxOrderReport
    {
        public string Instrument { get; set; }
        public string Currency { get; set; }
        public int OrderID { get; set; }
        public string Info { get; set; }
        public double Amount { get; set; }
    }
}
