using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Vendor
{
    public class Mas_Vendor
    {
        public long VendorId { get; set; } // numeric(18, 0)
        public string VendorId_Encrypted { get; set; }
        public bool? Deactivate { get; set; }
        public bool? UseBy { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string MachineName { get; set; }
        public long? CmpID { get; set; }
        public long? VendorBranchId { get; set; }

        public string VendorType { get; set; }
        public string CompanyName { get; set; }
        public string TradeName { get; set; }

        public string RegisteredAddress { get; set; }
        public string OperationAddress { get; set; }
        public DateTime? DateOfEstablishment { get; set; }

        public string GSTIN { get; set; }
        public string PAN { get; set; }
        public string TAN { get; set; }

        public double UDYAMorMSME { get; set; }
        public double EPFONumber { get; set; }
        public double LabourLicenseNumber { get; set; }

        public string ISOOrQualityCertificate { get; set; }
        public string NatureOfBusiness { get; set; }
        public string Description { get; set; }

        public bool IsHavingHirelink { get; set; }
        public string CustomerCode { get; set; }

        
    }

}