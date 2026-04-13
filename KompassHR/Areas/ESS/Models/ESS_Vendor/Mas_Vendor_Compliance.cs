using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Vendor
{
    public class Mas_Vendor_Compliance
    {
        public double VendorComplianceId { get; set; } // Unchecked (Required)
        public string VendorComplianceId_Encrypted { get; set; }
        public bool? Deactivate { get; set; }
        public bool? UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int VendorId { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public string ScopeOfServices { get; set; }
        public string SLADetails { get; set; }
        public string ServiceCharges { get; set; }
        public string PaymentTerms { get; set; }
        public string BankName { get; set; }
        public string IFSCCode { get; set; }
        public string CancelledCheckUpload { get; set; }
    }
}