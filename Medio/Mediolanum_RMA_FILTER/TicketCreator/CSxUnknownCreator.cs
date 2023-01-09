using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mediolanum_RMA_FILTER.TicketCreator.AbstractBase;
using RichMarketAdapter.ticket;
using sophis.log;

namespace Mediolanum_RMA_FILTER.TicketCreator
{
    class CSxUnknownCreator : BaseTicketCreator
    {
        private static string _ClassName = typeof(CSxBond2Creator).Name;

        public CSxUnknownCreator(eRBCTicketType type)
            : base(type)
        {
        }

        public override bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields)
        {
            using (Logger logger = new Logger(_ClassName, MethodBase.GetCurrentMethod().Name))
            {
                logger.log(Severity.error, "Unknown document class CSV received");
                base.SetTicketMessage(ref ticketMessage, fields);
                SetDefaultFailureMessageFileds(ref ticketMessage);
                return true;
            }
        }

    }
}
