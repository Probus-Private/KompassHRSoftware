using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy
{
    public class EmployeeVerifyAPI
    {
        public double VerifyId { get; set; }
        public string VerifyId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public bool IsAadhar { get; set; }
        public bool IsPAN { get; set; }
        public bool IsPassport { get; set; }
        public bool IsVoter { get; set; }
        public bool IsVehicleRC { get; set; }
        public bool IsDrivingLicence { get; set; }
        public bool IsEmployeement { get; set; }
        public bool IsGeoLocation { get; set; }
    }
}