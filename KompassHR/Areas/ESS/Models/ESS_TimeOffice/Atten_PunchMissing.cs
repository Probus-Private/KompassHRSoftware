using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class Atten_PunchMissing
    {
        public int PunchMissingID { get; set; }
        public string PunchMissingID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int PunchMissingEmployeeID { get; set; }
        public int PunchMissingBranchId { get; set; }
        public int DocNo { get; set; }
        public DateTime PunchMissingAttendanceDate { get; set; }
        public DateTime PunchMissingLogDate { get; set; }
        public Nullable<System.TimeSpan> PunchMissingLogTime { get; set; }
        public string PunchMissingInOut { get; set; }
        public int PunchMissingShiftId { get; set; }
        public string PunchMissingShiftName { get; set; }
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

        public bool ShiftFlag { get; set; }
        

    }
}