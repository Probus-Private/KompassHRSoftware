using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_ShiftChangeSetting
    {
        public double ShiftChangeSettingId { get; set; }
        public string ShiftChangeSettingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double ShiftChangeSettingBranchId { get; set; }
        public int? MonthlyLimit { get; set; }
        public int? BackDateDays { get; set; }
        public bool IsDefault { get; set; }
        public float? AutoApprovalDays { get; set; }
        public float? FutureDateDays { get; set; }
    }
}