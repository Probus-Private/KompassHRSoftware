using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Prime
{
    public class Mas_SubDepartment
    {
    
        public double SubDepartmentId { get; set; }
        public string SubDepartmentId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public string DepartmentId { get; set; }
        public string SubDepartmentName { get; set; }
        public bool UseBy { get; set; }
    }
}