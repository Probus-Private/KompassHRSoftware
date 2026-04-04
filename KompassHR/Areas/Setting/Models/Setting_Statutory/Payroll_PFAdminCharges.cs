using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_PFAdminCharges
    {
        public int PFAdminChargesId { get; set; }
        public string PFAdminChargesId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public float? PFEmployee { get; set; }
        public float? PFEmployeer { get; set; }
        public float? PFAC1 { get; set; }
        public float? PFAC10 { get; set; }
        public float? PFAC2 { get; set; }
        public float? PFAC21 { get; set; }
        public float? PFAC22 { get; set; }
        public int? PFAgeLimit { get; set; }
        public int? PFCeilingLimit { get; set; }
        public int? EPSCeilingLimit { get; set; }
        public int? EDLICeilingLimit { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

    }
}