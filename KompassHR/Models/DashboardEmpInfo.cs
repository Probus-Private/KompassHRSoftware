using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Models
{
    public class DashboardEmpInfo
    {
        public string TimeLine_Year { get; set; }
        public string TimeLine_Month { get; set; }
        public string TimeLine_Day { get; set; }
        public string TimeLine_DayOfWeek { get; set; }
        public string TimeLine_WeekOfCurrentYear { get; set; }
        public string TimeLine_DayOfYear { get; set; }
        public string TimeLine_QuarterOfYear { get; set; }
        public string EmployeeId { get; set; }
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public int EmployeeBranchId { get; set; }
        public string BranchName { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public string Gender { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public DateTime? ProbationDueDate { get; set; }
        public DateTime? BirthdayDate { get; set; }
        public int EmployeeDepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string EmployeeDesignationId { get; set; }
        public string DesignationName { get; set; }
        public string GradeName { get; set; }
        public string EmployeeGroupName { get; set; }
        public string EmployeeTypeName { get; set; }
        public string WeekOff1 { get; set; }
        public string WeekOff2 { get; set; }
        public string WeekOff2Week { get; set; }
        public string PrimaryMobile { get; set; }
        public string SecondaryMobile { get; set; }
        public string WhatsAppNo { get; set; }
        public string PersonalEmailId { get; set; }
        public int ReportingManager1Id { get; set; }
        public int ReportingManager2Id { get; set; }
        public int ReportingHRId { get; set; }
        public int ReportingAccountId { get; set; }
        public string ReportingManager1Name { get; set; }
        public string ReportingManager2Name { get; set; }
        public string ReportingHRName { get; set; }
        public string ReportingAccountName { get; set; }
        public string IsManager { get; set; }
        public string LastLoginTime { get; set; }
        public string LastPasswordChangeDayCount { get; set; }
        public string EmployeeFirstName { get; set; }
        public string TotalAge { get; set; }
        public string TotalExperience { get; set; }
        public string Remark1  { get; set; }
        public string Remark2  { get; set; }
        public string Remark3  { get; set; }
        public string Remark4  { get; set; }
        public string Remark5  { get; set; }
        public string Remark6  { get; set; }
        public string Remark7  { get; set; }
        public string Remark8  { get; set; }
        public string Remark9  { get; set; }
        public string Remark10 { get; set; }

        public string HeaderColor { get; set; }
        public string HeaderTextColor { get; set; }
    }
    public class IsInternational
    {
        public string Type { get; set; }
        public string Description { get; set; }
    }

    public class News
    {
        public int NewsID { get; set; }
        public string NewsTitle { get; set; }
        public string NewsDescripation { get; set; }
        public string FilePath { get; set; }
    }
    public class Events
    {
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public string EventDescripation { get; set; }
        public string FilePath { get; set; }
    }
    public class Announcement
    {
        public int AnnouncementID { get; set; }
        public string AnnouncementTitle { get; set; }
        public string AnnouncementDescripition { get; set; }
        public string FilePath { get; set; }
    }
    public class Reward
    {
        public int RewardID { get; set; }
        public string RewardTitle { get; set; }
        public string RewardDescripition { get; set; }
        public string FilePath { get; set; }
    }
}