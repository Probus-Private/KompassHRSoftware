using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_ESICContribution
    {
        public double ESICContributionId { get; set; }
        public string ESICContribution_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public float? ESICEmployee { get; set; }
        public float? ESICEmployer { get; set; }
        public float? ESICLimit { get; set; }
        public float? ESICLimitForPhysicalHandicapped { get; set; }
    }
}