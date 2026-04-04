using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Recruitment
{
    public class Recruitment_Interview_Skill
    {

        public  double InterviewSkillId { get; set; }
        public string InterviewSkillId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public string SkillName { get; set; }
        public string Description { get; set; }
    }
}