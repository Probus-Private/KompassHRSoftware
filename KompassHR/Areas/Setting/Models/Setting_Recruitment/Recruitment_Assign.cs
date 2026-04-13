using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Recruitment
{
    public class Recruitment_Assign
    {
        public double RecruitmentAssignID { get; set; }
        public string RecruitmentAssignID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double BranchId { get; set; }
        public double RecruitmentAssignEmployeeID { get; set; }
        public bool IsActive { get; set; }
    }
}