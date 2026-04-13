using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_PolicyAndLibrary
{
    public class Mas_PlantHR
    {
        public double PlantHRId { get; set; }
        public string PlantHRId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PlantHREmployeeID { get; set; }
        public string EmployeeName { get; set; }
    }
}