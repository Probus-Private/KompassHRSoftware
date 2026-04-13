using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class Payroll_MakerChecker
    {
        public double MakerCheckerId { get; set; }
        public string MakerChecker_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double MakerCheckerCmpId { get; set; }
        public double MakerCheckerBranchId { get; set; }
        public double TopManagerId { get; set; }
        public double PayrollMakerEmpId { get; set; }
        public double PayrollCheckerEmpId { get; set; }
        public double AccountMakerEmpId { get; set; }
        public double AccountCheckerEmpId { get; set; }
        public double IncrementMakerEmpId { get; set; }
        public double IncrementCheckerEmpId { get; set; }
        public bool IsTopManagerApprove { get; set; }
    }
}