using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_ESS
    {
        public double ESSId { get; set; }
        public string ESSId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ESSEmployeeId { get; set; }
        public string ESSLoginID { get; set; }
        public string ESSPassword { get; set; }
        public string ESSSecurityQuestion { get; set; }
        public string ESSAnswer { get; set; }
        public bool ESSIsActive { get; set; }
        public bool ESSIsLock { get; set; }
        public int ESSLoginAttemptCount { get; set; }
        public DateTime ESSLastLoginTime { get; set; }
        public DateTime ESSLastPasswordChange { get; set; }
        public bool IsExit { get; set; }
        public bool IsAdmin { get; set; }
        public double UserAccessPolicyId { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
        public string Answer { get; set; }
        public bool? IsMFA { get; set; }
        public string MFAToken { get; set; }
        public bool? MFAEnabled { get; set; }
        public bool IsApp { get; set; }

    }

    public class UserAccessPolicyHistory
    {
        public string ScreenDisplayMenuName { get; set; }
        public string ModuleName { get; set; }
        public string ScreenMenuType { get; set; }
        public string DescriptionForUser { get; set; }
    }
}