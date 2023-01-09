using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RichMarketAdapter.ticket;

namespace Mediolanum_RMA_FILTER.Interfaces
{
    interface IRBCTicketCreator
    {
        bool GetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields);

        bool SetTicketMessage(ref ITicketMessage ticketMessage, List<string> fields);

        void ValidateTicketFields(ref ITicketMessage ticketMessage, List<string> fields);
    }
}
