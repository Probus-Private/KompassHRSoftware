using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class AttendanceLock
    {
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime MonthYear { get; set; }
        public bool IsAttendanceLock { get; set; }
    }
}