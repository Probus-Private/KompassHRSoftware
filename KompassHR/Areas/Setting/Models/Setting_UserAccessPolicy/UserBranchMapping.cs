using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy
{
    public class UserBranchMapping
    {
        public double MapId { get; set; }
        public string MapId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double EmployeeID { get; set; }
        public double BranchID { get; set; }
        public double CmpID { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserPolicyMapping
    {
        public double MapId { get; set; }
        public string MapId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double EmployeeID { get; set; }
        public double BranchID { get; set; }
        public double CmpID { get; set; }
        public bool IsActive { get; set; }
    }

    public class PolicyMapping
    {
        public double MapPolicyId { get; set; }
        public double UserGroupId { get; set; }
        public bool IsActive { get; set; }
        public String UserGroupName { get; set; }
        public double EmployeeID { get; set; }
    }

    public class IsAdminAccess
    {
        public String EmployeeId { get; set; }
        public bool IsAdmin { get; set; }

        public String ESSId_Encrypted { get; set; }
        public String EmployeeName { get; set; }
    }
}