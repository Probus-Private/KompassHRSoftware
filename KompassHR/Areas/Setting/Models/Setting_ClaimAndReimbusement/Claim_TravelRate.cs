using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_ClaimAndReimbusement
{
    public class Claim_TravelRate
    {

        public int TravelRateId { get; set; }
        public string TravelRateId_Encrypted { get; set; }
        public string VehicalType { get; set; }
        public string TravelRateAmount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public string TravelRateBranchId { get; set; }
    }
}