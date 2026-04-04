using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Prime
{
    public class Mas_CompanyEmergencyContact
    {
        public double CompanyEmergencyContactId { get; set; }
        public string CompanyEmergencyContactId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double ContactBranchId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string MobileNo { get; set; }
        public string Designation { get; set; }
        public bool IsGlobal { get; set; }
       
    }
}