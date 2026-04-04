using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Leave
{
    public class Leave_EncashmentSetting
    {
        public double LeaveEncashSettingId { get; set; }
        public string LeaveEncashSettingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string CmpId { get; set; }
        public string LeaveEncashmentBranchId { get; set; }
        public double LeaveEncashSettingLeaveTypeId { get; set; }
        public string LeaveEncashSettingtLeaveSettingId { get; set; }
        public string LeaveEncashSettingEarningId { get; set; }
    }
}