using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_LMS
{
    public class LMS_Category_GroupMaster
    {
        public decimal CategoryGroupMasterId { get; set; }
        public string CategoryGroupMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; } // Nullable in case it's not always set
        public string MachineName { get; set; }
        public string LMSCategoryGroupName { get; set; }

        public decimal CategoryGroupCompanyId { get; set; }
        public decimal CategoryGroupBranchId { get; set; }
    }

    public class LMS_CategoryIds
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int IsActive { get; set; }
    }
}