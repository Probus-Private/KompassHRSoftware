using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class YearlyBonusPaid
    {
        public DateTime Month { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime MonthYear { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public int Amount { get; set; }
        public string Remark { get; set; }
    }
    public class YearlyBonusInput
    {
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string Amount { get; set; }
        public string Remark { get; set; }
        public DateTime Month { get; set; }
    }
}