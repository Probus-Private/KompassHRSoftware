using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Recruitment
{
    public class Recruitment_Interview_Result_ChkList
    {
        
        public double InterviewResultCheckListId { get; set; }
        public double InterviewResultCheckListRate { get; set; }
        public float InterviwerRate { get; set; }
        public string CreatedBy { get; set; }
        public string InterviewResultRemark { get; set; }
        public string MachineName { get; set; }
        public double InterviewResultInterviewId { get; set; }
    }
}