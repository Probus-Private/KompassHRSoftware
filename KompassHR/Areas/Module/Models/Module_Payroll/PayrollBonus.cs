using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class PayrollBonus
    {
        public int BonusId { get; set; }
        public string BonusId_Encrypted { get; set; }
        public int Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int BonusEmpId { get; set; }
        public int FinantialYear { get; set; }
        public int Amount { get; set; }
        public string Remark { get; set; }
        

        public int EmployeeId { get; set; }
        public DateTime MonthYear { get; set; }
        public string Month { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string PaidRemark { get; set; }
       public string PaidMonth { get; set; }


        public List<EmployeeBonusData> EmployeeBonus { get; set; }
    }

    public class EmployeeBonusData
    {
        public string BonusId { get; set; }
        public string BonusId_Encrypted { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public decimal Amount { get; set; }
        public string PaidRemark { get; set; }
        public string Month { get; set; }
        public string Remark { get; set; }
        // public int FinantialYear { get; set; }
    }


}