using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_LeaveEncashment
    {
        public int LeaveEncashmentID { get; set; }
        public string LeaveEncashmentID_Encrypted { get; set; }

        public int CmpId { get; set; }
        public int EmployeeId { get; set; }
        public int BranchId { get; set; }

        public string FinancialYear { get; set; }
        public string Amount { get; set; }

        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }

        public string Remark { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
    }
}