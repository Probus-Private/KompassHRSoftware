using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Vendor
{
    public class Mas_Vendor_ContactDetails
    {
        public double ContactID { get; set; }
        public string ContactID_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ContactVendorId { get; set; }
        public string PrimaryContact { get; set; }
        public string Designation { get; set; }
        public string EmailID { get; set; }
        public double MobileNo { get; set; }
        public double AlternateContactNo { get; set; }
        public double OfficialLandline { get; set; }
        public bool Deactivate { get; set; }
    }
}