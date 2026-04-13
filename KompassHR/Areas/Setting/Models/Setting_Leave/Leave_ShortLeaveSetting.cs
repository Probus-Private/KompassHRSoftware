using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Leave
{
    public class Leave_ShortLeaveSetting
    {
        [Key]
        public double ShortLeaveSettingID { get; set; }
        public string ShortLeaveSettingID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ShortLeaveSettingBranchId { get; set; }
        public string ShortLeaveSettingName { get; set; }
        public int? MonthlyLimit { get; set; }
        public int? Minute { get; set; }
        public int Direction { get; set; }
        public bool IsMultipleReqAllowInDay { get; set; }
        public int BackDateDays { get; set; }
        public float? AutoApprovalDays { get; set; }
    }
}