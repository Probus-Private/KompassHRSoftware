using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_IncomeTax
{
    public class IncomeTax_TaxSurchargeDetails
    {
        public double TaxSurcharge_MasterId { get; set; }
        //public double TaxSurchargeMasterId { get; set; }
        public string TaxSurchargeMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public float LowerLimit { get; set; }
        public float UpperLimit { get; set; }
        public double TaxFyearId { get; set; }
        public string TaxSurchargeType { get; set; }
        public double TaxSurchargeTypeId { get; set; }        
        //public double TaxSurchargeDetailId { get; set; }                  
        public float TaxSurcharge { get; set; }
        public float TaxSurcharge_AboveAmt { get; set; }
    }

    public class IncomeTax_Surcharge_Bulk
    {
        public float LowerLimit { get; set; }
        public float UpperLimit { get; set; }       
        public float TaxSurcharge { get; set; }        
        public float TaxSurcharge_AboveAmt { get; set; }
    }
}