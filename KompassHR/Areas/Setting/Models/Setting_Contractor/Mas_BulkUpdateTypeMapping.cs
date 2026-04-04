using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Contractor
{
    public class Mas_BulkUpdateTypeMapping
    {
        public double TypeMappingId { get; set; }
        public string TypeMappingId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public String ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double TypeId { get; set; }
        public double EmployeeId { get; set; }
        public double IsActive { get; set; }
      
    }
}