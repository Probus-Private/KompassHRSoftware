using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Ticket
{
    public class Ticket_Category
    {
        public double TicketCategoryID { get; set; }
        public string TicketCategoryID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string TicketCategoryName { get; set; }
        public bool IsActive { get; set; }
    }
}