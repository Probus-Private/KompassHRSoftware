using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Conference
{
    public class Conference_List
    {
        public double ConferenceListID { get; set; }
        public string ConferenceListID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        //public bool Deactivate { get; set; }
        //public string CreatedBy { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public string ModifiedBy { get; set; }
        //public string ModifiedDate { get; set; }
        //public string MachineName { get; set; }
        public string ConferenceName { get; set; }
        public string ConferenceDescription { get; set; }

    }
}