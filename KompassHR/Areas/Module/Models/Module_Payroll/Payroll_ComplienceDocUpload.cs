using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_ComplienceDocUpload
    {
        public double ComplienceDocUploadId { get; set; }
        public string ComplienceDocUploadId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }

        public double CmpId { get; set; }
        public double BranchID { get; set; }
        public DateTime? MonthYear { get; set; }
        public string FilePath { get; set; }
        public string Remark { get; set; }
        public double ComplienceDocTypeId { get; set; }
        public double ComplienceSubTypeId { get; set; }
    }
}