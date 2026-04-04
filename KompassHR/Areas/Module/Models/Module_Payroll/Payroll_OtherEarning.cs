using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_OtherEarning
    {

        public double VariableEarningId { get; set; }
        public string VariableEarningId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double? VariableEarningCmpId { get; set; }
        public double? VariableEarningBranchId { get; set; }
        public DateTime MonthYear { get; set; }
        public string VariableHead { get; set; }
        public double VariableEarningEmployeeId { get; set; }
        public string Amount { get; set; }
        public bool SalaryLock { get; set; }
        public string Remarks { get; set; }
        public string OtherEarningName { get; set; }
    }

    public class OtherEarning
    {
        public int OtherEarningEmployeeId { get; set; }
        public string OtherEarningAmount { get; set; }
        public string OtherEarningEarningremarks { get; set; }

    }
    public class OtherEarningList
    {
        public DateTime OtherEarningMonthYear { get; set; }
        public double CmpID { get; set; }
        public double BranchId { get; set; }
        public string EarningName { get; set; }

    }

    public class Other_Earning_Excel
    {
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string Amount { get; set; }
        public string Remark { get; set; }

    }
}