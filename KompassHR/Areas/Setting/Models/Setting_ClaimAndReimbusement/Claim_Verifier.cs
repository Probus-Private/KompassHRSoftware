using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_ClaimAndReimbusement
{
    public class Claim_Verifier
    {
        public int ClaimVerifierId { get; set; }
        public string ClaimVerifierId_Encrypted { get; set; }
        public Boolean Deactivate { get; set; }
        public Boolean UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int VerifierBranchId { get; set; }
        public int VerifierEmployeeId { get; set; }
        public string VerifierType { get; set; }
    }
}