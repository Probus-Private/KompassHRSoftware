using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_Salary
    {
        public int CmpId { get; set; }
    }

    public class SalaryProcess
    {
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public DateTime Month { get; set; }
        public bool LoadWithLog { get; set; }
    }
}