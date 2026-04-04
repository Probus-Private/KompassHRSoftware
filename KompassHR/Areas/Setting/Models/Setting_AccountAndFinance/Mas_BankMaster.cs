using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_AccountAndFinance
{
    public class Mas_Bank
    {
        [Key]
        public double BankId { get; set; }
        public string BankId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string BankName { get; set; }
        public bool BankUseBy { get; set; }
    }
}