using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TicketManagement
{
    public class Ticket_LevelTAT
    {
        public long TicketLevel_Id { get; set; }
        public string TicketLevel_Id_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public long CmpID { get; set; }
        public string Level { get; set; }
        public string TAT { get; set; }
    }
}