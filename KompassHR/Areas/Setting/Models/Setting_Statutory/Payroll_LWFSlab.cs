using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_LWFSlab
    {
        public double LWFSlabId { get; set; }
        public string LWFSlabId_Encrypted { get; set; }
        public double LWFSlabMasterId { get; set; }
        public string LWFSlabMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double LWFStateCode { get; set; }
        public float LowerLimit { get; set; }
        public float UpperLimit { get; set; }
        public float LWFEmployee { get; set; }
        public float LWFEmployer { get; set; }
        public string LWFDeductionMonth { get; set; }
        public string Remark { get; set; }
    }

    public class Payroll_LWFSlab_Month
    {
        public double LWFSlab_Month { get; set; }
        public bool IsApplicable { get; set; }
    }
    public class Payroll_LWFSlab_Master
    {
        public double LWFSlabMasterId { get; set; }
        public string LWFSlabMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double LWFStateCode { get; set; }
        public string Remark { get; set; }
    }


}