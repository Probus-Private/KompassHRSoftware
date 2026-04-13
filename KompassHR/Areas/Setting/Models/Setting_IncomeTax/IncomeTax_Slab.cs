using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_IncomeTax
{
    public class IncomeTax_Slab
    {
        public double TaxRateMasterId { get; set; }
        public string TaxRateMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public float LowerLimit { get; set; }
        public float UpperLimit { get; set; }
        public float PTAmount { get; set; }
        public string TaxRateType { get; set; }
        public double TaxRateTypeId { get; set; }
        public string TaxRateAge { get; set; }
        public double TaxRateAgeId { get; set; }
        public double TaxFyearId { get; set; }
        public float TaxRateAmount { get; set; }
        public float TaxSurcharge { get; set; }
        public float TaxRatePercentage { get; set; }
        public decimal TaxRate_AboveAmt { get; set; }

    }


    public class IncomeTax_Slab_Bulk
    {
        public decimal LowerLimit { get; set; }
        public decimal UpperLimit { get; set; }
        public decimal TaxRateAmount { get; set; }
        public decimal TaxSurcharge { get; set; }
        public decimal TaxRatePercentage { get; set; }
        public decimal TaxRate_AboveAmt { get; set; }

    }

}