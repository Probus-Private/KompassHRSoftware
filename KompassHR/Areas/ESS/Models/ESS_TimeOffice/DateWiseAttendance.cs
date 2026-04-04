using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class DateWiseAttendance
    {
        public int? EmployeeID { get; set; }
        public int? BranchId { get; set; }
        public int? CmpId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Process { get; set; }
    }

    public class DateWiseAttendanceList
    {
        public DateTime Date { get; set; }
        public TimeSpan InTime { get; set; }
        public TimeSpan OutTime { get; set; }
        public string Shift { get; set; }
        public string Status { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan OverTime { get; set; }
        public int LateComing { get; set; }
        public TimeSpan LateMark { get; set; }
        public TimeSpan EarlyGoing { get; set; }
        public int NoOffCoff { get; set; }
        public string Remark { get; set; }
    }

    public class DateWiseAttendanceReg
    {
        public string EmployeeId { get; set; }
        public string Date { get; set; }
        public string Day { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string Status { get; set; }
        public string ShiftId { get; set; }
        public string Shift { get; set; }
        public string TotalDuration { get; set; }
        public string OverTime { get; set; }
        public string LateMark { get; set; }
        public string LateComing { get; set; }
        public string EarlyGoing { get; set; }
        public string NoOffCOFF { get; set; }
        public string Remark { get; set; }
    }
}