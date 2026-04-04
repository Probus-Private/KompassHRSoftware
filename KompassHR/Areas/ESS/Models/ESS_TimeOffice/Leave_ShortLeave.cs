using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class Leave_ShortLeave
    {
        public double ShortLeaveId { get; set; }
        public string ShortLeaveId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int ShortLeaveEmployeeId { get; set; }
        public int ShortLeaveBranchId { get; set; }
        public int DocNo { get; set; }
        public DateTime ShortLeaveDate { get; set; }
        public int ShortLeaveShiftId { get; set; }
        public string ShiftName { get; set; }
        public string Direction { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public int TotalMinutes { get; set; }
        public string TotalDuration { get; set; }
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
        public Nullable<System.DateTime> ApproveRejectDate { get; set; }
        public bool CancelFlag { get; set; }
        public int CancelBy { get; set; }
        public string CancelReason { get; set; }
        public Nullable<System.DateTime> CancelDate { get; set; }
        public string RequestFrom { get; set; }
        public string AdditionNotify { get; set; }


    }
}