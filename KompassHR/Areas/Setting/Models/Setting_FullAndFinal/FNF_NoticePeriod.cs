using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{
    public class FNF_NoticePeriod
    {
        [Key]
        public double NoticePeriodId { get; set; }
        public string NoticePeriodId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }    
        public double NoticePeriodGradeId { get; set; }
        public string NoticePeriodDays { get; set; }
        public string GradeName { get; set; }
        public double DepartmentID { get; set; }
        public double DesignationID { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }

    }
}