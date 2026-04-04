using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class Atten_PersonalGatepass
    {
        public double PersonalGatepassID { get; set; }
        public string PersonalGatepassID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double PersonalGatepassEmployeeId { get; set; }
        public double PersonalGatepassBranchId { get; set; }
        public int DocNo { get; set; }
        public double PersonalGatepassShiftId { get; set; }
        public string PersonalGatepassShiftName { get; set; }
        public string Direction { get; set; }
        public DateTime PersonalGatepassDate { get; set; }
        public Nullable<System.DateTime> FromTime { get; set; }
        public Nullable<System.DateTime> ToTime { get; set; }
        public string Reason { get; set; }
        public char Status { get; set; }
        public bool ApprovedAutoManually { get; set; }
        public double ManagerId1 { get; set; }
        public double ManagerId2 { get; set; }
        public double HRId { get; set; }
        public string ManagerId1View { get; set; }
        public string ManagerId2View { get; set; }
        public string HRIdView { get; set; }
        public double ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public DateTime ApproveRejectDate { get; set; }
        public bool CancelFlag { get; set; }
        public double CancelBy { get; set; }
        public string CancelReason { get; set; }
        public DateTime CancelDate { get; set; }
        public string RequestFrom { get; set; }
        public string AdditionNotify { get; set; }
        public string TotalDurations { get; set; }
        public double EmployeeID { get; set; }
    }
}