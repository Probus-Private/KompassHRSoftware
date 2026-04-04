using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance
{
    public class Payroll_StopAdvance
    {
        public double StopAdvanceID { get; set; }
        public string StopAdvanceID_Encrypted { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double StopAdvanceEmployeeID { get; set; }
        public DateTime MonthYear { get; set; }
        public string Remark { get; set; }
    }

    public class Payroll_StopAdvance_Bulk
    {
       
        public double StopAdvanceEmployeeID { get; set; }
        public double AdvanceId { get; set; }
        public string Remark { get; set; }
    }
}