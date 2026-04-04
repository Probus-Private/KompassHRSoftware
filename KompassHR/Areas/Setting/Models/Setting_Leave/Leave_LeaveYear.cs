using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Leave
{
    public class Leave_Year
    {
        [Key]
        public double LeaveYearID { get; set; }
        public string LeaveYearID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double SiteCode { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsActivate { get; set; }
        public bool IsDefault { get; set; }
        public string branchname { get; set; }

    }
}