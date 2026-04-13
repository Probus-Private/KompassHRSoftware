using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ClaimReimbusement
{
    public class Claim_Advance
    {
        public int AdvanceClaimId { get; set; }
        public string AdvanceClaimId_Encrypted { get; set;}
        public Boolean Deactivate { get; set; }
        public int CmpId { get; set; }
        public int AdvanceClaimBranchId { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public int AdvanceClaimEmployeeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public int AdvancePurposeId { get; set; }
        public double AdvanceAmount { get; set; }
        public double ApprovedAmount { get; set; }
        public string Status { get; set; }
        public int AdvanceClaimManagerId1 { get; set; }
        public int AdvanceClaimManagerId2 { get; set; }
        public int AdvanceClaimAccountId { get; set; }
        public int ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public DateTime ApproveRejectDate { get; set; }
        public int NoOfLevelOfApproval { get; set; }
        public int CurrentStatusOfApproval { get; set; }
        public Boolean CancelFlag { get; set; }
        public int CancelBy { get; set; }
        public string CancelStatus { get; set; }
        public string CancelReason { get; set; }
        public DateTime CancelDate { get; set; }
        public string RequestFrom { get; set; }
        public int AdvanceClaimAccountBy { get; set; }
        public string AdvanceClaimAccountRemark { get; set; }
    }
}
