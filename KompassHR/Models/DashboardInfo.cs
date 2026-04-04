using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Models
{
    public class DashboardInfo
    {
        public string ESSDashboard_id { get; set; }
        public string EmployeeId { get; set; }
        public string LeaveCount { get; set; }
        public string ShortLeaveCount { get; set; }
        public string CoffCount { get; set; }
        public string OutDoorCompanyCount { get; set; }
        public string PersonalGatepassCount { get; set; }
        public string PunchMissingCount { get; set; }
        public string ShiftChangeCount { get; set; }
        public string VisitorAppointmentCount { get; set; }
        public string FNF_EmployeeResignationCount { get; set; }
        public string MonthBirthdayTeamCount { get; set; }
        public string MonthBirthdayDepartmentCount { get; set; }
        public string MonthBirthdayFavouriteCount { get; set; }
        public string TodayBirthdayTeamCount { get; set; }
        public string TodayBirthdayDepartmentCount { get; set; }
        public string TodayBirthdayFavouriteCount { get; set; }
        public string TodayBirthdayMissedCount { get; set; }
        public string NewJoineeTeam { get; set; }
        public string NewJoineeDepartment { get; set; }
        public string TicketTotal { get; set; }
        public string TicketOpen { get; set; }
        public string TicketReview { get; set; }
        public string TicketClose { get; set; }
        public string CurrentOpening { get; set; }
        public string WarningsTotal { get; set; }
        public string WarningsThisMonth { get; set; }
        public string MyTeamMember { get; set; }
        public string MyTeamMemberMale { get; set; }
        public string MyTeamMemberFemale { get; set; }
        public string MyDepartmentMember { get; set; }
        public string MyDepartmentMemberMale { get; set; }
        public string MyDepartmentMemberFemale { get; set; }
        public string TotalBU { get; set; }
        public string CoffGeneratedCount { get; set; }
    }
}