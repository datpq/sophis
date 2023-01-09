using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using sophis.oms;
using sophis.utils;
using sophis.OrderGeneration.PortfolioColumn;
using MEDIO.OrderAutomation.NET.Source.Column;
using MEDIO.CORE.Tools;
using sophis.instrument;
using Sophis.OrderBookCompliance;
using sophis.OrderGeneration.DOB.Builders;

namespace MEDIO.OrderAutomation.NET.Source.OrderCreationValidator
{
    public class CSxOrderCreationMiscValidator :  IOrderCreationValidator
    {
        static bool? checkBreachColumn = null;

        internal static bool CheckBreachColumn()
        {
            if( checkBreachColumn.HasValue == false)
            {
                try
                {
                    string sql = "SELECT CONFIG_VALUE from MEDIO_TKT_CONFIG where CONFIG_NAME = 'CHECK_REBALANCING_BREACH'";
                    object res = CSxDBHelper.GetOneRecord(sql);
                    if( res != null)
                    {
                        int resInt = Convert.ToInt32(res);
                        if( resInt == 0)
                        {
                            checkBreachColumn = false;
                        }
                        else
                        {
                            checkBreachColumn = true;
                        }
                    }
                    else
                    {
                        //By default we use the check
                        checkBreachColumn = true;
                    }
            
                }
                 catch(Exception ex)
                {

                }
            }
            return checkBreachColumn.Value;
        }

        public ValidationResult Validate(IOrder order, bool creating)
        {
            using (var LOG = new CSMLog())
            {
                LOG.Begin(this.GetType().Name, MethodBase.GetCurrentMethod().Name);

                if (order.Side != ESide.Sell && order.Side != ESide.Buy)
                {
                    string msg = "Only buy/sell orders are supported";
                    return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() { msg } };
                }

                if (order.BookingType != EBookingType.Regular)
                {
                    string msg = "Booking type is not supported. Please change to 'Regular'";
                    return new ValidationResult() { IsValid = false, ValidationMessages = new List<string>() { msg } };
                }

                if (DynamicOrderBuilder.Instance.IsSessionActive && true == CheckBreachColumn())
                {
                    if( CSxRebalancingConstraint.fListOfBreachedInstFolio.Count > 0 )
                    {
                        List<string> validationMessages = new List<string>();

                        foreach( Tuple<int,int> oneBreach in CSxRebalancingConstraint.fListOfBreachedInstFolio)
                        {
                            
                            CSMInstrument breachedInst = CSMInstrument.GetInstance(oneBreach.Item1);
                            if( breachedInst == null)
                            {
                                continue;
                                    
                            }
                            using (CMString instName = breachedInst.GetName())
                            {
                                string msg = "Rebalancing constraint breached for instrument = " + instName.ToString();
                                validationMessages.Add(msg);
                            }
                            
                        }
                        return new ValidationResult() { IsValid = false, ValidationMessages = validationMessages};
                    }
                }

                return new ValidationResult() { IsValid = true };
            }
        }
    }
}
