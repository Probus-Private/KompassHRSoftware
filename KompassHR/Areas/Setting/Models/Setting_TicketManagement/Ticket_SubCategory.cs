using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TicketManagement
{
    public class Ticket_SubCategory
    {
        public long TicketSubCategoryID { get; set; }  
        public string TicketSubCategoryID_Encrypted { get; set; } 
        public long CmpID { get; set; } 
        public long BranchID { get; set; } 
        public bool Deactivate { get; set; } 
        public string CreatedBy { get; set; } 
        public DateTime CreatedDate { get; set; } 
        public string ModifiedBy { get; set; } 
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public long TicketCategory_Id { get; set; }
        public string TicketSubCategoryName { get; set; }
    }
}