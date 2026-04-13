using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Leave
{
    public class Leave_Setting
    {
        public double LeaveSettingId { get; set; }
        public string LeaveSettingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double LeaveSettingLeaveGroupId { get; set; }
        public double LeaveSettingLeaveTypeId { get; set; }
        public float? YearlyLeave { get; set; }
        public bool IsMonthlyLimit { get; set; }
        public float MonthlyMaxLeave { get; set; }
        public bool IsCarryforward { get; set; }       
        public bool CarryforwardBalanceOrMax { get; set; }
        public float CarryforwardMaxLeave { get; set; }
        public bool IsEncashment { get; set; }
        public bool EncashmentdBalanceOrMax { get; set; }
        public float EncashmentMaxLeave { get; set; }
        public string Color { get; set; }
        public bool IsSandwich { get; set; }
        public bool IsAfterConfirmation { get; set; }
        public bool IsActivate { get; set; }
        public bool IsDocRequired { get; set; }
        public int IsAfterDays { get; set; }
        public bool IsAutoApproval { get; set; }
        public int IsAutoApprovalAfterDays { get; set; }
        public bool IsHalfDayAllow { get; set; }
        public float? AutoApprovalDays { get; set; }

        public int? BackDateDays { get; set; }
        public int? FutureDateDays { get; set; }

        public float MinLeave { get; set; }
        public float MaxLeave { get; set; }

        public bool IsMonthlyCreadit { get; set; }
    }
}