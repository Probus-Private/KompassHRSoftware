using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Onboarding
{
    public class Mas_CriticalStageCategory
    {
        public double CriticalStageId { get; set; }
        public string CriticalStageId_Encrypted { get; set; }
        public string CriticalStageName { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double LineMasterID { get; set; }
        public string Description { get; set; }
       
    }
}