using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_EmployeeGrievance
{
    public class Employee_Grievance_SubCategory
    {
        public double GrievanceSubCategoryId { get; set; }
        public string GrievanceSubCategoryId_Encrypted { get; set; }
        public double GrievanceCategoryId { get; set; }
        public string GrievanceCategory { get; set; }
        public string GrievanceSubCategory { get; set; }
        public string Description { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
    }
}