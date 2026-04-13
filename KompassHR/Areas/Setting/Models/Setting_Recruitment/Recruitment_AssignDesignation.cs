using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Recruitment
{
    public class Recruitment_AssignDesignation
    {
        public double RecruitmentAssignDesignationID { get; set; }
        public string RecruitmentAssignDesignationID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double AssignDesignationEmployeeID { get; set; }
        public double RecruitmentDesignationID { get; set; }
        public double RecruitmentGradeID { get; set; }
        public int NoticePeriodDays { get; set; }
        public bool IsActive { get; set; }
      



    }
    public class AssignDesignation
    {
        public int RecruitmentDesignationID { get; set; }
        public bool IsActive { get; set; }
        public int AssignDesignationEmployeeID { get; set; }

    }
}