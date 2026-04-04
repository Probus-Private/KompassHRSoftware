using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_PTSlab
    {
        public double PTSlabId { get; set; }
        public string PTSlabId_Encrypted { get; set; }
        public double PTSlabMasterId { get; set; }
        public string PTSlabMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PTStateCode { get; set; }
        public float LowerLimit { get; set; }
        public float UpperLimit { get; set; }
        public float PTAmount { get; set; }
        public float FebPTAmount { get; set; }
        public string StateName { get; set; }
        public string Remark { get; set; }
        public double PTSlabMonth { get; set; }
        public string PTDeductionType { get; set; }
    }

    public class Payroll_PTSlab_Master
    {
        public double PTSlabMasterId { get; set; }
        public string PTSlabMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PTStateCode { get; set; }
        public string Remark { get; set; }
    }

    public class Payroll_PTSlab_Month
    {
        public double PTSlab_Month { get; set; }
        public bool IsApplicable { get; set; }
    }

}