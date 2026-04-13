using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_TDSDeduction
    {
        public double TDSDeductionId { get; set; }
        public string TDSDeductionId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double? TDSDeductionCmpId { get; set; }
        public double? TDSBranchId { get; set; }
        public DateTime MonthYear { get; set; }
        public double TDSEmployeeId { get; set; }
        public string Amount { get; set; }
        public bool SalaryLock { get; set; }
        public string Remarks { get; set; }
    }
    public class TDSDeduction_Excel
    {
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string Amount { get; set; }
        public string Remark { get; set; }
        public int EmployeeBranchId { get; set; }
    }

}