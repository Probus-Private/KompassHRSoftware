using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class Payroll_Company_Bank
    {
        public double CompanyBankId { get; set; }
        public string CompanyBankId_Encrypted { get; set; }
        public double CompanyBankCmpID { get; set; }
        public double CompanyBankBUId { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string IFSCCode { get; set; }
        public string AccountNo { get; set; }
        public bool IsDefault { get; set; }

        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string CategoryName { get; set; }

    }
}