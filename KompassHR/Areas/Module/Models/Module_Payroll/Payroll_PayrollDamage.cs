using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_PayrollDamage
    {
        public double PayrollDamageID { get; set; }
        public string PayrollDamageID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PayrollDamageBranchID { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime PayrollDamageMonthYear { get; set; }
        public double PayrollDamageEmployeeId { get; set; }
        public float PayrollDamageAmount { get; set; }
        public bool PayrollDamageSalaryLock { get; set; }
        public string PayrollDamageRemarks { get; set; }
        public double CmpID { get; set; }
    }
    public class PayrollDamage
    {
        public string PayrollDamageEmployeeId { get; set; }
        public string PayrollDamageAmount { get; set; }
        public string PayrollDamageRemarks { get; set; }

    }
    public class Payroll_Damage_List
    {
        public DateTime? PayrollDamageMonthYear { get; set; }

    }

}