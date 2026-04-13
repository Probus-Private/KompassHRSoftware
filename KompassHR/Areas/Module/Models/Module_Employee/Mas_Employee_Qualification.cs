using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Qualification
    {
        public double QualificationID { get; set; }
        public string QualificationID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double QualificationEmployeeID { get; set; }
        public string QualificationName { get; set; }
        public string QualificationType { get; set; }
        public string QualificationUniversity { get; set; }
        public string QualificationPassingYear { get; set; }
        public string QualificationMark { get; set; }
        public string QualificationRollNo { get; set; }
    }
}