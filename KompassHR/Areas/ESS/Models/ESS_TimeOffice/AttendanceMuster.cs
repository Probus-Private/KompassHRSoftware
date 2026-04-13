using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class AttendanceMuster
    {
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public DateTime Month { get; set; }
        public int EmployeeId { get; set; }
        public int DaysInMonth { get; set; }
    }

    //public class Attendance
    //{
    //    public DateTime AttendanceDate { get; set; }
    //    public int EmployeeId { get; set; }
    //    public string  { get; set; }
    //}
}