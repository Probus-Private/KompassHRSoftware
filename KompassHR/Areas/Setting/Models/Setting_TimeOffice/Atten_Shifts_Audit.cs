using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_Shifts_Audit
    {
        public double ShiftsAuditId { get; set; }
        public string ShiftsAuditId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ShiftsAuditIdCmpId { get; set; }
        public double ShiftsAuditBranchId { get; set; }
        public double ShiftsAuditShiftId { get; set; }
        public float Percentage { get; set; }
    }

    public class _Shifts_Audit
    {
        public double ShiftsAuditShiftId { get; set; }
        public float Percentage { get; set; }
    }
}