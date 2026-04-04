using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_EventAndNews
{
    public class Event_Event
    {
        public double EventID { get; set; }
        public string EventID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string EventTitle { get; set; }
        public string EventDescripation { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public string FilePath { get; set; }
        public bool IsActive { get; set; }
    }
}