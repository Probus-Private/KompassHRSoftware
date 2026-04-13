using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class Payroll_PayrollCycle
    {

        public double PayrollCycleId { get; set; }
        public string PayrollCycleId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double PayrollCycleBranchId { get; set; }
        public bool IsMonthDays { get; set; }
        public bool IsPeriodBasedDays { get; set; }
        public double PeriodBasedFromDay { get; set; }
        public double PeriodBasedToDay { get; set; }
        public string PayrollCycleFixedDays { get; set; }
        public string PayrollCycleType { get; set; }
        public double adjustDays {get;set;}
    }
}