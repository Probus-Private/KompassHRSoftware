using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Training
{
    public class Training_TrainingAssesmentQuestion
    {
        public double Training_AssesmentQuestionId { get; set; }
        public string Training_AssesmentQuestionId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double TrainingCalenderId { get; set; }
        public string TrainingCalenderName { get; set; }
        public double TrainingLangaugeId { get; set; }
        public int QuestionType { get; set; }
        public string Question { get; set; }
        public string Options { get; set; }
        public bool IsActive { get; set; }
        public bool IsAnswer { get; set; }
        public string InputAnswer { get; set; }

        public double QustionID { get; set; }
        public double OptionId { get; set; }
        public string IsSelected { get; set; }
        public double OptionType { get; set; }
        public string InputRemark { get; set; }
        public string OptionName { get; set; }
        public double MasterID { get; set; }
    }
    public class Training_TrainingAssesmentQuestion_option
    {
        public string Options { get; set; }
        public bool IsAnswer { get; set; }
        public string InputAnswer { get; set; }
    }
}
