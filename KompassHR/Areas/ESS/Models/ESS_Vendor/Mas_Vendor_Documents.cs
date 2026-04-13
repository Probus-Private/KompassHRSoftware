using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Vendor
{
    public class Mas_Vendor_Documents
    {
        public double VendorDocumentId { get; set; } // Unchecked (Required)
        public string VendorDocumentId_Encrypted { get; set; }
        public bool? Deactivate { get; set; }
        public bool? UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public long? VendorId { get; set; }
        public string GSTCertificate { get; set; }
        public string PANCard { get; set; }
        public string EPFOCerificate { get; set; }
        public string LabourLicense { get; set; }
        public string CancelledCheque { get; set; }
        public string CompanyProfile { get; set; }
        public string SignedAgreement { get; set; }
    }
}