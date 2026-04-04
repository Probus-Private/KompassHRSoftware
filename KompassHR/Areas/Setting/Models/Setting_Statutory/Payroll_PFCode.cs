using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_PFCode
    {
        public int PFCodeId { get; set; }
        public string PFCodeId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
       // public double PFCodeBranchId { get; set; }
        public string PFCode { get; set; }        
        public string PFAddress { get; set; }
        public string PFRemark { get; set; }
    }
}