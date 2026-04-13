using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy
{
    public class UsermappingCompanyandbranch
    {
        public double CompanyId { get; set; }
        public string CompanyName { get; set; }
        public double BranchId { get; set; }
        public string BranchName { get; set; }
    }
}