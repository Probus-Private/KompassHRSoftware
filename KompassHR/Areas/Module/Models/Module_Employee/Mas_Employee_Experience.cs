using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Experience
    {

        public double ExperienceID { get; set; }
        public string ExperienceId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ExperienceEmployeeID { get; set; }
        public string CompanyName { get; set; }
        public string IndustryType { get; set; }
        public string HRName { get; set; }
        public string ManagerName { get; set; }
        public string HREmailID { get; set; }
        public string ManagerEmailID { get; set; }
        public string HRContact { get; set; }
        public string ManagerContact { get; set; }
        public string CompanyAddress { get; set; }
        public string Desigantion { get; set; }
        public string Department { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Salary { get; set; }
        public string JobDescription { get; set; }         
       
    }
}