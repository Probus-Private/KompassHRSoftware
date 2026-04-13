using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ClaimReimbusement
{
    public class Claim_GeneralClaim
    {
        public string GeneralClaimId { get; set; }
        public string GeneralClaimId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string CmpId { get; set; }
        public string GeneralBranchId { get; set; }
        public string GeneralClaimEmployeeId { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GeneralClaimExpenseCategoryID { get; set; }
        public float GeneralClaimAmount { get; set; }
        public string GeneralClaimDescription { get; set; }
        public string AttachmentPath { get; set; }
        public float ApprovedAmount { get; set; }
        public string Status { get; set; }
        public string GeneralClaimManagerID1 { get; set; }
        public string GeneralClaimManagerID2 { get; set; }
        public string GeneralClaimAccountID { get; set; }
        public string GeneralClaimHRID { get; set; }
        public string ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public DateTime ApproveRejectDate { get; set; }
        public bool GeneralClaimCancelFlag { get; set; }
        public string GeneralClaimCancelBy { get; set; }
        public string GeneralClaimCancelReason { get; set; }
        public DateTime GeneralClaimCancelDate { get; set; }
        public string RequestFrom { get; set; }
        public string GeneralClaimAccountBy { get; set; }
        public string GeneralClaimAccountRemark { get; set; }
        public DateTime GeneralClaimAccountDate { get; set; }


    }
}