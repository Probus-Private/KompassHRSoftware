using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_PerquisiteAmount
    {
        public double PerquisiteAmountId { get; set; }
        public string PerquisiteAmountId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PerquisiteFyearId { get; set; }
        public double PerquisiteEmployeeId { get; set; }
        public double PerquisiteListId { get; set; }
        public double TotalAmount { get; set; }
        public double TaxableAmount { get; set; }
        public DateTime MonthYear { get; set; }

    }
}