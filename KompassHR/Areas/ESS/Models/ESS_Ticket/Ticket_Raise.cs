using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Models.ESS_Ticket
{
    public class Ticket_Raise
    {
        public double TicketRaiseID { get; set; }
        public string TicketRaiseID_Encrypted { get; set; }
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
        public string TicketRaiseTitle { get; set; }

        [AllowHtml]
        public string Description { get; set; }

        public string FilePath { get; set; }
        public string Priority { get; set; }
        public double TicketRaiseEmployeeID { get; set; }
        public double TicketRaisedTo { get; set; }
        public string AttachFile { get; set; }
        public double TicketSubCategoryID { get; set; }
        public double Level1EmployeeID { get; set; }
        public string Level1Status { get; set; }
        public string Level1Remark { get; set; }
        public double Level2EmployeeID { get; set; }
        public string Level2Status { get; set; }
        public string Level2Remark { get; set; }
        public double Level3EmployeeID { get; set; }
        public string Level3Status { get; set; }
        public string Level3Remark { get; set; }
        public double Level4EmployeeID { get; set; }
        public string Level4Status { get; set; }
        public string Level4Remark { get; set; }
        public double TotalLevel { get; set; }
        public double CurrentLevel { get; set; }
        public DateTime LatestAssignDateTime { get; set; }
        public string LatestAssignSolution { get; set; }


        [AllowHtml]
        public string KeyPoints { get; set; }
    }
}