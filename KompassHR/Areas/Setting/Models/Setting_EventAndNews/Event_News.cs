using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_EventAndNews
{
    public class Event_News
    {
        public double NewsID { get; set; }
        public string NewsID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string NewsTitle { get; set; }
        public string NewsDescripation { get; set; }
        public DateTime NewsFromDate { get; set; }
        public DateTime NewsToDate { get; set; }
        public bool IsActive { get; set; }
        public string FilePath { get; set; }
    }
}