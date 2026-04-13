using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Ticket
{
    public class Ticket_Solution
    {
        public double TicketSolutionID { get; set; }

        public string TicketSolutionID_Encrypted { get; set; }
        public double CmpID { get; set; }

        public double BranchID { get; set; }

        public bool Deactivate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string MachineName { get; set; }

        public double TicketCategoryID { get; set; }

        public int DocNo { get; set; }

        public DateTime DocDate { get; set; }

        public double TicketRaiseID { get; set; }

        public double TicketResponserEmployeeID { get; set; }

        public string Solution { get; set; }

        public string FilePath { get; set; }

        public string Status { get; set; }

        public string CompeletedPercentage { get; set; }
        public string AttachFile { get; set; }

    }
}