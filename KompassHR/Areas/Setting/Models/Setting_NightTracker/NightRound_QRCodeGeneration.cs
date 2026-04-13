using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_NightTracker
{
    public class NightRound_QRCodeGeneration
    {

        public double QRCodeID { get; set; }
        public string QRCodeID_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public int LocationID { get; set; }
        public string QRCodeLocationName { get; set; }
        public bool IsActive { get; set; }
        public string LocationName { get; set; }
        
    }
}