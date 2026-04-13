using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Dashboards.Models
{
    public class Recruitment_Resume
    {
        public double ResumeId { get; set; }
        public string ResumeId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ResumeJobOpeningId { get; set; }
        public double ResumeReferEmployeeId { get; set; }
        public string CandidateName { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public string Gender { get; set; }
        public string TotalExperience { get; set; }
        public string Age { get; set; }
        public string CTC { get; set; }
        public string CurrentCity { get; set; }
        public string CurrentWorking { get; set; }
        public string ResumePath { get; set; }
        public string ResumeStatus { get; set; }
    }
}