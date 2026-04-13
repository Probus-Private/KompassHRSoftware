using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Recruitment
{
    public class Recruitment_Interview
    {
        public double InterviewId { get; set; }
        public string InterviewId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int InterviewResourceId { get; set; }
        public int InterviewResourceEmployeeId { get; set; }
        public int InterviewPoolEmployeeId { get; set; }
        public int InterviewResumeId { get; set; }
        public string InterviewType { get; set; }
        public string InterviewLocation { get; set; }
        public string MeetingLink { get; set; }
        public int InterviewerEmployeeId { get; set; }
        public string InterviewAccepted { get; set; }
        public string InterviewStatus { get; set; }
        public string Reason { get; set; }
        public string InterviewResult { get; set; }
        public DateTime InterviewDate { get; set; }
        public DateTime InterviewTime { get; set; }
        public string ResumeId_Encrypted { get; set; }
        public string CandidateMail { get; set; }
        public string ResumeId { get; set; }
        public int ReuestPoolRecruiterId { get; set; }
        public string designationname { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string CandidateName { get; set; }
        public int ResourceId { get; set; }
    }
}