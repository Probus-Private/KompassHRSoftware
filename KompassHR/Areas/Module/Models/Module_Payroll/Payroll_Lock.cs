using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_Lock
    {
        public double PayrollLockId { get; set; }
        public string PayrollLockId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double PayrollLockBranchID { get; set; }
        public DateTime PayrollLockMonthYear { get; set; }
        public bool Status { get; set; } = true;
    }
}