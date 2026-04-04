using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Leave
{
    public class YearlyLeaveEncashment
    {
        public int CompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveGroupId { get; set; }
        public int LeaveTypeId { get; set; }
        public string Leave { get; set; }
        public string LeaveYear { get; set; }
    }
}