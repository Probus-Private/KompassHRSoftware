using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Recruitment
{
    public class Recruitment_JobDescriptionTemplate
    {
        public double RecruitmentJDTemplateID { get; set; }
        public string RecruitmentJDTemplateID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string RecruitmentJDTemplate { get; set; }
        public double JDTemplateDepartmentId { get; set; }
        public string Description { get; set; }
        public string DepartmentName { get; set; }
    }
}