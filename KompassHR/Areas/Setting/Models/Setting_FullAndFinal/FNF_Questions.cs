using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{
    public class FNF_Questions
    {
        public double FeedbackID { get; set; }
        public string FeedbackID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double DesignationID { get; set; }
        public double DepartmentID { get; set; }
        public string Question { get; set; }
        public double QuestionTypeID { get; set; }
        public string Options { get; set; }
        public bool IsActive { get; set; }
        public bool SpecificDepartment { get; set; }
        public string FeedbackFor { get; set; }

    }

    public class FNF_Questions_option
    {
        public string Options { get; set; }
    }
}