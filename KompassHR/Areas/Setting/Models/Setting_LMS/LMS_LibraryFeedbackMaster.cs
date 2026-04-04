using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_LMS
{
    public class LMS_LibraryFeedbackMaster
    {
        public double LMSFeedbackID { get; set; }
        public string LMSFeedbackID_Encrypted { get; set; }
        public string Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string QuestionType { get; set; }
        public string Question { get; set; }
        public bool IsActive { get; set; }
        public double LMSLibraryId { get; set; }
        public double LMSLibraryId_Encrypted { get; set; }
        public string Options { get; set; }
        public bool IsAnswer { get; set; }
        public string InputAnswer { get; set; }
    }

    public class LMS_LibraryFeedbackDetails_option
    {
        public string Options { get; set; }
        public bool IsAnswer { get; set; }
        public string InputAnswer { get; set; }
    }

}