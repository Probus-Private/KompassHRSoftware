using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_PMS
{
    public class PMS_EmployeeReporting
    {
        public double PmsEmployeeReportingId { get; set; }
        public string PmsEmployeeReportingId_Encrypted { get; set; }

        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public string MachineName { get; set; }

        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double EmployeeId { get; set; }

        public double ReportingManagerId1 { get; set; }
        public double ReportingManagerId2 { get; set; }
        public double ReportingManagerId3 { get; set; }
        public double ReportingManagerId4 { get; set; }
    }

}