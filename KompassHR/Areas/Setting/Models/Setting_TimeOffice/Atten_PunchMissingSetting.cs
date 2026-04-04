using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_PunchMissingSetting
    {
        public double PunchMissingSettingId { get; set; }
        public string PunchMissingSettingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double PunchMissingSettingBranchId { get; set; }
        public string PunchMissingSettingName { get; set; }
        public int? MonthlyLimitForIn { get; set; }
        public int? MonthlyLimitForOut { get; set; }
        public int? BackDateDays { get; set; }
        public string MonthlyLimitType { get; set; }
        public int MonthlyRequestLimit { get; set; }
        public bool IsDefault { get; set; }
        public float? AutoApprovalDays { get; set; }
        public bool AllowForAfterShiftEndTime { get; set; }
        public bool AllowForBeforeShiftStartTime { get; set; }
        public bool InOutApplySameDay { get; set; }

        //public double? PunchMissingSettingID { get; set; }
        //public string PunchMissingSetting_Encrypted { get; set; }
        //public double? CmpID { get; set; }
        //public double? SiteCode { get; set; }
        //public bool Deactivate { get; set; }
        //public string CreatedBy { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public string ModifiedBy { get; set; }
        //public string ModifiedDate { get; set; }
        //public string MachineName { get; set; }
        //public String PunchMissingSettingName { get; set; }
        //public double? MonthlyLimitForIn { get; set; }
        //public double? MonthlyLimitForOut { get; set; }
        //public double? BackDateDays { get; set; }
    }
}