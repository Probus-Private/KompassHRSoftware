using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_PayrollFine
    {
        public double PayrollFineID { get; set; }
        public string PayrollFineID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PayrollFineBranchID { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime PayrollFineMonthYear { get; set; }   
        public double PayrollFineEmployeeId { get; set; }
        public float PayrollFineAmount { get; set; }
        public bool PayrollFineSalaryLock { get; set; }
        public string PayrollFineRemarks { get; set; }
        public double CmpID { get; set; }
    }
    public class Payroll_Fine
    {
        public string PayrollFineEmployeeId { get; set; }
        public string PayrollFineAmount { get; set; }
        public string PayrollFineRemarks { get; set; }

    }

    public class Payroll_Fine_List
    {
        public DateTime? PayrollFineMonthYear { get; set; }

    }
}