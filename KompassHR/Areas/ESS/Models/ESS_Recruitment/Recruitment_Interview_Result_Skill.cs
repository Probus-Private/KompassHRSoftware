using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Recruitment
{
    public class Recruitment_Interview_Result_Skill
    {
        public string Interview_Skill_Name { get; set;}
        public string Interview_Skill_Description { get; set; }
        public string CreatedBy { get; set; }
        //public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public double InterviewResultSkillInterviewId { get; set; }
    }
}