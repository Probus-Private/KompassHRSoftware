using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class Atten_OutDoorCompany
    {
        public int OutDoorCompanyID { get; set; }
        public string OutDoorCompanyID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int OutDoorCompanyEmployeeID { get; set; }
        public int OutDoorCompanyBranchId { get; set; }
        public int DocNo { get; set; }
        public string OutDoorCompanyType { get; set; }
        public string Type { get; set; }
        public int OutDoorCompanyShiftId { get; set; }
        public string OutDoorCompanyShiftName { get; set; }
        public string Direction { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Nullable<System.TimeSpan> FromTime { get; set; }
        public Nullable<System.TimeSpan> ToTime { get; set; }
        public int TotalMinutes { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public bool ApprovedAutoManually { get; set; }
        public int ManagerId1 { get; set; }
        public int ManagerId2 { get; set; }
        public int HRId { get; set; }
        public string ManagerId1View { get; set; }
        public string ManagerId3View { get; set; }
        public string HRIdView { get; set; }
        public int ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public DateTime ApproveRejectDate { get; set; }
        public bool CancelFlag { get; set; }
        public int CancelBy { get; set; }
        public string CancelReason { get; set; }
        public DateTime CancelDate { get; set; }
        public string RequestFrom { get; set; }
        public string AdditionNotify { get; set; }
        public string TotalDuration { get; set; }
        public string TotalDays { get; set; }
        public double VisitingBranchID { get; set; }
        public bool IsNightShift { get; set; }

    }
}