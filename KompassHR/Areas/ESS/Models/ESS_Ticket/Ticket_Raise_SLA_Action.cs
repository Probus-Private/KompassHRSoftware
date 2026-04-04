using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Ticket
{
    public class Ticket_Raise_SLA_Action
    {

            public double TicketRaiseSLAActionId { get; set; }
        
            public string TicketRaiseSLAActionId_Encrypted { get; set; }
        
            public bool Deactivate { get; set; }

            public string CreatedBy { get; set; }

            public DateTime CreatedDate { get; set; }

            public string ModifiedBy { get; set; }

            public DateTime ModifiedDate { get; set; }

            public string MachineName { get; set; }


            public int DocNo { get; set; }

            public DateTime DocDate { get; set; }

            public double SLAAction_TicketRaiseID { get; set; }

            public double AssignedTo { get; set; }

          public double TicketRaiseEmployeeID { get; set; }

            public string Solution { get; set; }

            public string FilePath { get; set; }

            public string Status { get; set; }
        
            public DateTime SolutionDatetime { get; set; }

            public string AttachFile { get; set; }


    }
}