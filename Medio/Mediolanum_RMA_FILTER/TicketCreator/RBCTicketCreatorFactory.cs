using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mediolanum_RMA_FILTER.TicketCreator;
using Mediolanum_RMA_FILTER.Tools;
using RichMarketAdapter.ticket;
using sophis.log;
using sophis.utils;

namespace Mediolanum_RMA_FILTER.Interfaces
{
    class RBCTicketCreatorFactory
    {
        private static string _ClassName = typeof(RBCTicketCreatorFactory).Name;

        public static bool CreateTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            IRBCTicketCreator ticketCreator = GetTicketCreator(fields);
            if (ticketCreator != null)
            {
                return ticketCreator.GetTicketMessage(ref ticketMessage, fields);
            }
            return false;
        }

        public static IRBCTicketCreator GetTicketCreator( List<string> fields, bool IsCAMerger = false)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                eRBCTicketType type = CSxRBCHelper.GetRBCTicketType(fields);
                logger.log(Severity.info, "RBC file type = " + type);
                switch (type)
                {
                    case eRBCTicketType.Bond:
                    case eRBCTicketType.Equity:
                    case eRBCTicketType.Loan:
                    case eRBCTicketType.Fund:
                    {
                        return new CSxEquityLikeCreator(type);
                    }
                    case eRBCTicketType.Bond2:
                    {
                        return new CSxBond2Creator(type);
                    }
                    case eRBCTicketType.Forex:
                    {
                        return new CSxForexCreator(type);
                    }
                    case eRBCTicketType.Option:
                    {
                        return new CSxOptionCreator(type);
                    }
                    case eRBCTicketType.Future:
                    {
                        return new CSxFutureCreator(type);
                    }
                    case eRBCTicketType.Cash:
                    {
                        return new CSxCashCreator(type);
                    }
                    case eRBCTicketType.Invoice:
                    {
                        return new CSxInvoiceCreator(type);
                    }
                    case eRBCTicketType.ForexHedge:
                    {
                        return new CSxForexHedgeCreator(type);
                    }
                    case eRBCTicketType.TACash:
                    {
                        return new CSxTACashCreator(type);
                    }
                    case eRBCTicketType.Swap:
                    {
                        return new CSxSwapCreator(type);
                    }
                    case eRBCTicketType.CorporateAction:
                    {
                        if (IsCAMerger)
                            return new CSxCAMergerLikeCreator(type);
                        else
                            return new CSxCorporateActionCreator(type);
                    }
                    case eRBCTicketType.Collateral:
                    {
                        return new CSxCollateralCreator(type);
                    }
                    case eRBCTicketType.TermDeposit:
                    {
                        return new CSxTermDepositCreator(type);
                    }
                    case eRBCTicketType.AllCustodyTrans:
                        return new CSxAllCustodyTransCreator(type);
                    case eRBCTicketType.GenericTrade:
                        return new CSxGenericTrade(type);
                    //Adding The new Ticket Creator
                    case eRBCTicketType.SSBOTCDataPoints:
                        return new CSxSSBOTCDataPointsCreator(type);
                    case eRBCTicketType.Unknown:
                    default:
                        return new CSxUnknownCreator(type);
                }
            }
        }
    }
}
