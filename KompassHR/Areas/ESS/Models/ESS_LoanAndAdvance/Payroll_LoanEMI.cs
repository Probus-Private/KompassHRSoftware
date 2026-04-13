using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance
{
    public class Payroll_LoanEMI
    {
        public long EMIId { get; set; }
        public string EMIId_Encrypted { get; set; }
        public bool? Deactivate { get; set; }
        public bool? UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public long? LoanId { get; set; }
        public long? EmployeeId { get; set; }
        public DateTime? MonthYear { get; set; }
        public string LoanAmount { get; set; }   // since it is nchar(10)
        public decimal? MonthlyEMIAmount { get; set; }
        public decimal? PrincipleAmount { get; set; }
        public decimal? InterestAmount { get; set; }
        public decimal? ClosingBalance { get; set; }
        public string Status { get; set; }
    }

}