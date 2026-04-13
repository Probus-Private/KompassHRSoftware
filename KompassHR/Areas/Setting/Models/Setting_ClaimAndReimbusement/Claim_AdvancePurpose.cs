using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_ClaimAndReimbusement
{
    public class Claim_AdvancePurpose
    {
        public double AdvancePurposeID { get; set; }
        public string AdvancePurposeId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string AdvancePurpose { get; set; }
    }
}