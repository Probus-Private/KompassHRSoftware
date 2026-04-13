using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_ShiftRule
    {
        public double ShiftRuleId { get; set; }
        public string ShiftRuleId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double ShiftRuleBranchId { get; set; }
        public string ShiftRuleName { get; set; }
        public string ShiftRuleSName { get; set; }
        public string OTFormula { get; set; }
        public string MinOT { get; set; }
        public bool IsMaxOT { get; set; }
        public float MaxOT { get; set; }
        public float FullDayDuration { get; set; }
        public float HalfDayDuration { get; set; }
        public bool IsMarkHalfDayForLate { get; set; }
        public float HalfDayLateByMins { get; set; }
        public bool IsMarkHalfdayForEarlyGoing { get; set; }
        public float HalfDayEarlyGoingMins { get; set; }
        public bool IsDefault { get; set; }
        public string OTCalculation { get; set; }

    }
}