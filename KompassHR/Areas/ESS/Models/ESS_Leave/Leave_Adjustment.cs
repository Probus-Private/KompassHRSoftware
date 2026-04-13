using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Leave
{
    public class Leave_Adjustment
    {
        public int LeaveAdjustmentId { get; set; }
        public string LeaveAdjustmentId_Encrypted { get; set; }
        public int CmpID { get; set; }
        public int LeaveAdjustmenBranchId { get; set; }
        public int LeaveAdjustmentYearId { get; set; }
        public int LeaveAdjustmentSettingId { get; set; }
        public int LeaveAdjustmentLeaveGroupId { get; set; }
        public int LeaveAdjustmentLeaveTypeId { get; set; }
        public int LeaveAdjustmentEmployeeId { get; set; }
        public float NoOfLeave { get; set; }
        public string AdjustmentRemark { get; set; }
        public string Remark { get; set; }
    }
}