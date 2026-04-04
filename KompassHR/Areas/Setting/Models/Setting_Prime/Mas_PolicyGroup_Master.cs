using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Prime
{
    public class Mas_PolicyGroup_Master
    {

        public double PolicyGroupMasterId { get; set; }
        public string PolicyGroupMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double PolicyGroupBranchId { get; set; }
        public string GroupName { get; set; }
        public string GroupShiftName { get; set; }
    }
    public class PolicyGroup
    {
        public int PolicyLibraryId { get; set; }
        public string PolicyName { get; set; }
        public string remark { get; set; }
    }
}