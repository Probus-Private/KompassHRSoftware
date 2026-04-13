using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_PMS
{
    public class PMS_Competencies
    {
        public int CompetencyId { get; set; }
        public string CompetencyId_Encrypted { get; set; }
        public bool? Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int? CmpID { get; set; }
        public int? CompetencyFrameworkId { get; set; }
        public string CompetencyName { get; set; }
        public string Description { get; set; }
        public double Weightage { get; set; }
    }
}