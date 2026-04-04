using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class Payroll_Master_Head
    {
        public string HeadName { get; set; }
        public string EncryptedId { get; set; }
        public decimal HeadId { get; set; }
        public string ShortName { get; set; }
        public string EarningDeductionType { get; set; }
        public string CTCType { get; set; }
        public string CTCTypeName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
    }

    public class HeadMapping
    {
        public string CompanyName { get; set; }
        public string HeadName { get; set; }
        
    }
}