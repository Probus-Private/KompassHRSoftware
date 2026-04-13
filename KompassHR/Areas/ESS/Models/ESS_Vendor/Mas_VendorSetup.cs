using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Vendor
{
    public class Mas_VendorSetup
    {
            public double VendorSetupId { get; set; }
            public string VendorSetupId_Encrypted { get; set; }
            public bool? Deactivate { get; set; }
            public bool? UseBy { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? CreatedDate { get; set; }
            public string ModifiedBy { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public string MachineName { get; set; }
            public long VendorId { get; set; }

            public bool? SetupBasicDetails { get; set; }
            public bool? SetupContactDetails { get; set; }
            public bool? SetupCompliance { get; set; }
            public bool? SetupCapacityDomain { get; set; }
            public bool? SetupDocumentUpload { get; set; }
            public bool? SetupVendorAccess { get; set; }
        
    }
}