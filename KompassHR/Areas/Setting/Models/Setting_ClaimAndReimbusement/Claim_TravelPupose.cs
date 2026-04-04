using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_ClaimAndReimbusement
{
    public class Claim_TravelPurpose
    {
        public double TravelPurposeID { get; set; }
        public string TravelPurposeId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string TravelPurpose { get; set; }

    }
}