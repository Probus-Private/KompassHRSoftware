using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_ESICExemptedAllowances_Master
    {
        public int ESICExemptedallowancesMasterId { get; set; }
        public string ESICExemptedallowancesMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }

    }

    public class Payroll_ESICExemptedAllowances_Detail
    {
        public string ESICExemptedallowancesName { get; set; }
        public string TempTableESICExemptedallowancesName { get; set; }
    }
}