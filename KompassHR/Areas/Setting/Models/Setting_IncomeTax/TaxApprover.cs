using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_IncomeTax
{
    public class TaxApprover
    {
        public double TaxApproverId { get; set; }
        public string TaxApproverId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double TaxApproverEmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyMobileNo { get; set; }
        public string CompanyEmailID { get; set; }
        public string BranchName { get; set; }
    }
}