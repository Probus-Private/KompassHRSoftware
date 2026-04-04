using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Vendor
{
    public class Mas_Vendor_CapacityDomain
    {
        public double VendorCapacityId { get; set; }

        public string VendorCapacityId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public long VendorId { get; set; }
        public string IndustriesServed { get; set; }
        public string SkillSetAvailable { get; set; }
        public string NoOfEmployeeSupplied { get; set; }
        public string LocationCovered { get; set; }
        public string BackgroundVerification { get; set; }
        public bool PoliceVerification { get; set; }
        public string OnBoardingSupport { get; set; }
    }
}