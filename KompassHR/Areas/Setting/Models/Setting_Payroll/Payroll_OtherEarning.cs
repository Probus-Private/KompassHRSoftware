using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class Payroll_OtherEarning
    {
        public double OtherEarningId { get; set; }
        public string OtherEarningId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public string VariableName { get; set; }
        public string EarningCategory { get; set; }
    }
}