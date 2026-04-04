using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class AttendanceRegularizationold
    {
        public DateTime CurrDate { get; set; }
        public int Id { get; set; }
        public string IOTime { get; set; }
        public string Statuss { get; set; }
    }

    public class AttendanceRegularization
    {
        public DateTime AttendanceDate { get; set; }
        public string Duration { get; set; }
        public string EarlyBy { get; set; }
        public int EmployeeID { get; set; }
        public int Id { get; set; }
        public string InOutShift { get; set; }
        public string InOutStatus { get; set; }
        public string InTime { get; set; }
        public string LateBy { get; set; }
        public string OT { get; set; }
        public string OutTime { get; set; }
        public string NetHrs { get; set; }
    }
}