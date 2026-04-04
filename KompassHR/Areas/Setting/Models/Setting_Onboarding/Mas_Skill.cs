using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Onboarding
{
    public class Mas_Skill
    {
        public double SkillID { get; set; }
        public string SkillID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string SkillName { get; set; }
        public double SkillDepartmentID { get; set; }
        public bool checkboxAll { get; set; }
    }
}