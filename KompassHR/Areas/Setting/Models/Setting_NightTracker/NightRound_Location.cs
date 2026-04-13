using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_NightTracker
{
    public class NightRound_Location
    {
        public double LocationID { get; set; }
        public string LocationId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public string LocationName { get; set; }
        public bool IsActive { get; set; }
        public bool UseBy { get; set; }

    }
}