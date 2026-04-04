using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy
{
    public class Tool_UserAccessPolicyMaster
    {
        public double UserGroupId { get; set; }
        public string UserGroupId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string UserGroupName { get; set; }
        
        public string ModuleName { get; set; }
        public bool Allcheckbox { get; set; }
    }
}