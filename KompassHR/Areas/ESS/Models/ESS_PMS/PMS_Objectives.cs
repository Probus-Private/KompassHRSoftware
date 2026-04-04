using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_Objectives
    {
        public long ObjectiveId { get; set; } 
        public long CmpId { get; set; }
        public string ObjectiveId_Encrypted { get; set; } 
        public bool? Deactivate { get; set; } 
        public bool? UseBy { get; set; } 
        public string CreatedBy { get; set; } 
        public DateTime? CreatedDate { get; set; } 
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; } 
        public string MachineName { get; set; } 
        public string Origin { get; set; } 
        public int? EmployeeId { get; set; } 
        public string ObjectiveTitle { get; set; }
    }
}