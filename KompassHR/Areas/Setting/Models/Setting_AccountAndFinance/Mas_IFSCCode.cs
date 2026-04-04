using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_AccountAndFinance
{
    public class Mas_IFSCCode
    {
        [Key]
        public double IFSCID { get; set; }
        public string IFSCID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double IFSCCodeBankID { get; set; }
        public string IFSCode { get; set; }
        public string BranchName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string StateName { get; set; }
        public string BankName { get; set; }

    }
}