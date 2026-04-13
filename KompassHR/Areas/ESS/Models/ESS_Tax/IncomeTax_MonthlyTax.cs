using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_MonthlyTax
    {
        public double MonthlyTaxId { get; set; }
        public string MonthlyTaxId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double MonthlyTaxFyearId { get; set; }
        public double MonthlyTaxEmployeeId { get; set; }
        public DateTime MonthlyTaxMonthYear { get; set; }
        public double MonthlyTaxGross { get; set; }
        public double MonthlyTaxAmount { get; set; }
        public double MonthlyTaxBaseTax { get; set; }
        public double MonthlyTaxSurcharge { get; set; }
        public double MonthlyTaxEducation { get; set; }
        public string MonthlyTaxRemark { get; set; }

    }
}