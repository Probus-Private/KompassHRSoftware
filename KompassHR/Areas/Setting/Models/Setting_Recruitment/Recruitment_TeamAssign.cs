using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Recruitment
{
    public class Recruitment_TeamAssign
    {
        public double Recruitment_TeamAssignID { get; set; }
        public string Recruitment_TeamAssignID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double RecruitmentHeadEmployeeId { get; set; }
        public double TeamAssignEmployeeId { get; set; }
       
    }

    public class Recruitment_TeamAssign_Employee
    {
        public string EmployeeName { get; set; }
        public double EmployeeId { get; set; }


    }
}