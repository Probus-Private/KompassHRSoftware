using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_SalaryHold
    {
        public int PayrollHoldId { get; set; }
        public string PayrollHoldId_Encrypted { get; set; }
        public int Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double Payroll_SalaryId { get; set; }
        public double Payroll_SalaryEmployeeId { get; set; }
        public DateTime Payroll_SalaryMonth { get; set; }
        public string Payroll_SalaryAmount { get; set; }
        public bool IsHold { get; set; }
        public DateTime HoldDate { get; set; }
        public string HoldRemark { get; set; }
        public double HoldEmployeeId { get; set; }
        public bool IsRelease { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseRemark { get; set; }
        public double ReleaseEmployeeId { get; set; }

        public bool IsHoldRelease { get; set; }
        public string HoldReleaseRemark { get; set; }
    }


    public class Payroll_SalaryHold_Bulk
    {
        public int PayrollHoldId { get; set; }
        public string PayrollHoldId_Encrypted { get; set; }

        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double Payroll_SalaryId { get; set; }
        public double Payroll_SalaryEmployeeId { get; set; }
        public DateTime Payroll_SalaryMonth { get; set; }
        public string Payroll_SalaryAmount { get; set; }
        public bool IsHold { get; set; }
        public DateTime HoldDate { get; set; }
        public string HoldRemark { get; set; }
        public double HoldEmployeeId { get; set; }
        public bool IsRelease { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseRemark { get; set; }
        public double ReleaseEmployeeId { get; set; }
        public string Type { get; set; }
    }

    public class Payroll_SalaryHold_Bulk_excel
    {
        public string Remark { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string SalaryBranchId { get; set; }
    }

}