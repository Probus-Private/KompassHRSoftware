using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Preboarding
{
    public class Onboarding_Instruction
    {
        public double PreInsructionId { get; set; }
        public string PreInsructionId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string Remark { get; set; }
        public int CmpID { get; set; }
        public string CompanyName { get; set; }

    }
}