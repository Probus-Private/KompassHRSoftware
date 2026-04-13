using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class AttenCycle
    {
        public double AttendanceCycleId { get; set; }
        public string AttendanceCycleId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double AtdLockIDBranchId { get; set; }
        public string AtdType { get; set; }
        public string FromDay { get; set; }
        public string ToDay { get; set; }
        public string PayrollCalculation { get; set; }
    }
}