using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_Arrears
    {
        public double ArrearsId { get; set; }
        public string ArrearsId_Encrypted { get; set; }
        public double ArrearsCmpId { get; set; }
        public double ArrearsBranchId { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }

        public double ArrearsEmployeeId { get; set; }
        public string ArrearsType { get; set; }
        public DateTime ArrearsCalculateMonthYear { get; set; }
        public DateTime ArrearsPaidMonthYear { get; set; }
        public double PayableDays { get; set; }
        public double PayableOTHrs { get; set; }
        public double ArrearsPayableDays { get; set; }
        public double ArrearsPayableOTHrs { get; set; }
        public double ArrearsActualAmount { get; set; }
        public double ArrearsAmount { get; set; }
        public string Remark { get; set; }
        public bool ArrearsPF { get; set; }
        public bool ArrearsPT { get; set; }
        public bool ArrearsESI { get; set; }
        public bool IsMergeWithSalary { get; set; }

        public int MonthDays { get; set; }
        public int GrossPay { get; set; }
        public int NetPays { get; set; }

    }
}