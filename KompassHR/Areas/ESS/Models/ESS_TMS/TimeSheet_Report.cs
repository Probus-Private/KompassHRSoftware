using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TMS
{
    public class TimeSheet_Report
    {
        public int CmpId { get; set;}
        public int ClientId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime MonthYear { get; set; }
    }
}