using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_PFWages_Master
    {
        public int PFWagesMasterId { get; set; }
        public string PFWagesMasterId_Encrypted  { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public string PFWagesRemark { get; set; }
    }

    public class Payroll_PFWages_Detail
    {
        public string PFWagesName { get; set; }
        public string PFWagesName_TempTable { get; set; }
    }
}