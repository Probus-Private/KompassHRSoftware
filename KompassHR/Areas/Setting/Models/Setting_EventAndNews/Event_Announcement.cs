using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_EventAndNews
{
    public class Event_Announcement
    {
        public double AnnouncementID { get; set; }
        public string AnnouncementID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public double AnnounceEmployeeID { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string AnnouncementTitle { get; set; }
        public string AnnouncementDescripition { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string FilePath { get; set; }
        public bool IsActive { get; set; }
    }
}