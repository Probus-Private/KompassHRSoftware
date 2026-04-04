using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_OtherDeduction
    {
        public double VariableDeductionId { get; set; }
        public string VariableDeductionId_Encrypted { get; set; }
        public string Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double? VariableDeductionCmpId { get; set; }
        public double? VariableDeductionBranchId { get; set; }
        public DateTime MonthYear { get; set; }
        public string VariableHead { get; set; }
        public double VariableDeductionEmployeeId { get; set; }
        public string Amount { get; set; }
        public bool SalaryLock { get; set; }
        public string Remarks { get; set; }
        public string OtherDeductionName { get; set; }

    }
    public class Other_Deduction
    {
        public string OtherDeductionEmployeeId { get; set; }
        public string OtherDeductionAmount { get; set; }
        public string OtherDeductionRemarks { get; set; }

    }

    public class Other_Deduction_List
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public string VariableHead { get; set; }
        public DateTime OtherDeductionMonthYear { get; set; }

    }

    public class Other_Deduction_Excel
    {
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string Amount { get; set; }
        public string Remark { get; set; }
        public int SalaryBranchId { get; set; }
    }

}