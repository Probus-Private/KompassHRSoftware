using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance
{
    public class Payroll_AdvanceInstallment
    {
        public double AdvanceInstallmentID { get; set; }
        public string AdvanceInstallmentID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double AdvanceInstallmentEmployeeId { get; set; }
        public double AdvanceId { get; set; }
        public string AdvanceInstallmentAmount { get; set; }
        public DateTime FromMonthYear { get; set; }
        public DateTime ToMonthYear { get; set; }
        public string Remark { get; set; }
    }
}