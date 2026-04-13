using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_MaleFemaleRate
    {
        public int MaleFemaleRateId { get; set; }
        public string MaleFemaleRateId_Encrypted { get; set; }
        public int Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public int MaleFemaleRateCmpid { get; set; }
        public int MaleFemaleRateBranchid { get; set; }
        public string Gender { get; set; }
        public int Rate { get; set; }
        public int InsumentRate { get; set; }
        public DateTime RateFromDate { get; set; }
        public DateTime RateTodate { get; set; }
        
    }
}