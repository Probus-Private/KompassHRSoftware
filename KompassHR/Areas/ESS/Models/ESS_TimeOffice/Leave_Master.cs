using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class Leave_Master
    {
        public double LeaveMasterId { get; set; }
        public string LeaveMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double LeaveMasterEmployeeId { get; set; }
        public double LeaveMasterBranchId { get; set; }
        public int DocNo { get; set; }
        public double LeaveMasterLeaveYearId { get; set; }
        public double LeaveMasterLeaveTypeId { get; set; }
        public double LeaveMasterLeaveGroupId { get; set; }
        public double LeaveMasterLeaveSettingId { get; set; }
        public string LeaveMasterDayType { get; set; }
        public double LeaveMasterShiftId { get; set; }
        public string LeaveMasterShiftName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public float TotalDays { get; set; }
        public string Reason { get; set; }
        public string AdditionNotify { get; set; }
        //Extra For bind ViewBag in DropDown
        public string LeaveYear { get; set; }
        public string EmployeeName { get; set; }
        public string ProofUpload { get; set; }

    }
}