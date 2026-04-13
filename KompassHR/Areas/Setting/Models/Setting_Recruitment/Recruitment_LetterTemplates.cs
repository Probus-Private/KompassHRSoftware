using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Recruitment
{
    public class Recruitment_LetterTemplates
    {
        public double LetterTemplateId { get; set; }
        public string LetterTemplateId_Encrypted { get; set; }

        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string MachineName { get; set; }

        public double CmpId { get; set; }
        public double DocOrigin { get; set; }

        public string TemplateSubject { get; set; }
        public string TemplateBody { get; set; }
    }
}