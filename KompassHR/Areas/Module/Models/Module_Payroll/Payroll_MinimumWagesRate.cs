using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_MinimumWagesRate
    {
        public double MinimumWageRatetId { get; set; }
        public string MinimumWageRatetId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double WageCategoryId { get; set; }
        public string Basic { get; set; }
        public string DA { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}