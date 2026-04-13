using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class PayrollAttendance
    {
        public string EmployeeNo { get; set; }
        public string BusinessUnit { get; set; }
        public string Department { get; set; }
        public string Grade { get; set; }
        public string BusinessArea { get; set; }
        public int MonthDays { get; set; }
    }
}