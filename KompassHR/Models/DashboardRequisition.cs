using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Models
{
    public class DashboardRequisition
    {
        public int LeaveCount { get; set; }
        public int ShortLeaveCount { get; set; } 
        public int OutDoorCompanyCount { get; set; }
        public int PersonalGatepassCount { get; set; }
        public int PunchMissingCount { get; set; }
        public int FNF_EmployeeResignationCount { get; set; }
        public int TodayBirthdayTeamCount { get; set; }
        public int TodayBirthdayDepartmentCount { get; set; }
        public int TodayBirthdayFavouriteCount { get; set; }
        public int TodayBirthdayMissedCount { get; set; }
        public int NewJoinee { get; set; }
    }
}