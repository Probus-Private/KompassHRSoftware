using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Onboarding
{
    public class Mas_LineMaster
    {
        public double LineId { get; set; }
        public string LineId_Encrypted { get; set; }
        public string LineName { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public bool IsDefault { get; set; }
    }
}