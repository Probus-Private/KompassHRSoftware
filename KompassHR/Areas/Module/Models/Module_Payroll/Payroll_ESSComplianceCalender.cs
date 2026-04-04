using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_ESSComplianceCalender
    {
        public int ComplianceCalenderId { get; set; }
        public string ComplianceCalenderId_Encrypted { get; set; }
        public int Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public int ComplianceId { get; set; }
        public int ComplianceCategoryId { get; set; }
        public string Type { get; set; }
        public string Remark { get; set; }
        public DateTime? EndDate { get; set; }
        public string Attachment { get; set; }



    }
}
  