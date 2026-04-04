using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy
{

    public class Tool_UserRightsInsertUpdate
    {
        public int UserRightsScreenId { get; set; }
        public int UserRightsModuleId { get; set; }
        public bool IsMenu { get; set; }
        public int EmployeeId { get; set; }

    }
    public class Tool_UserRights
    {

        public int UserRightsID { get; set; }
        public string UserRightsID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double UserRightsEmployeeId { get; set; }
        public double UserRightsScreenId { get; set; }
        public double UserRightsModuleId { get; set; }
        public double UserRightsGroupId { get; set; }

        public bool IsMenu { get; set; }

        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
        public bool IsSave { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }
        public bool IsList { get; set; }
    }

    public class ReplicationUserRight
    {
        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
        public double PolicyId { get; set; }
        public int EmployeeId { get; set; }
    }
}