using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Atten_Cycle
    {
        public int AttendanceCycleId { get; set; }
        public string AttendanceCycleId_Encrypted { get; set; }
        public int Deactivate { get; set; }
        public int UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int AtdLockIDBranchId { get; set; }
        public string AtdType { get; set; }
        public DateTime FromDay { get; set; }
        public DateTime ToDay { get; set; }
        public string PayrollCalculation { get; set; }
        public string PayrollCalculationValue { get; set; }

    }
}