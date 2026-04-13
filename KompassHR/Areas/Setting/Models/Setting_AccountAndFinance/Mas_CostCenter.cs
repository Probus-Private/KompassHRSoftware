using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_AccountAndFinance
{
    public class Mas_CostCenter
    {
        [Key]
        public double CostCenterId { get; set; }
        public string CostCenterId_Encrypted { get; set; }
        public double CmpID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string CostCenterName { get; set; }
        public bool UseBy { get; set; }
        public bool IsDefault { get; set; }

    }
}