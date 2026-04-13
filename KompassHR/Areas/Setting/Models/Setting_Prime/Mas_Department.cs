using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Prime
{
    public class Mas_Department
    {
        [Key]
        public double DepartmentId { get; set; }
        public string DepartmentId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public string DepartmentName { get; set; }
        public bool UseBy { get; set; }
        public bool IsSubDepartment { get; set; }
    }
}