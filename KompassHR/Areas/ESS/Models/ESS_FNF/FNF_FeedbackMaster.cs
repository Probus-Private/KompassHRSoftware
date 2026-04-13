using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_FNF
{
    public class FNF_FeedbackMaster
    {
        public double FeedbackID { get; set; }
        public double QuestionID { get; set; }
        public double FNF_FeedbackMaster_FeedbackID { get; set; }
        public double FeedDetailsID { get; set; }
        public double FNF_Feedback_Employee_MasterId { get; set; }
        public double FNF_FeedbackDetails_FeedDetailsID { get; set; }
        public double OptionType { get; set; }
        public double QuestionType { get; set; }
        public bool OptionSelected { get; set; }
        public string InputRemark { get; set; }
        public string FeedbackID_Encrypted { get; set; }
        public bool Deactivate { get; set; }     
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double FeedBackEmployeeID { get; set; }
        public double OptionID { get; set; }
        public string Selected { get; set; }

    }

    public class FNF_FeedbackMaster_Question
    {
        public double FeedbackID { get; set; }
    }

    public class FNF_FeedbackMaster_Answer
    {
        public double QustionID { get; set; }
        public double OptionId { get; set; }
        public string IsSelected { get; set; }
        public double OptionType { get; set; }
        public string InputRemark { get; set; }
        public string OptionName { get; set; }
        public double MasterID { get; set; }
    }
}