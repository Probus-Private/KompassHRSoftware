using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Conference
{
    public class Curve_Reminder
    {
        public double ReminderID { get; set; }
        public string ReminderID_Encrypted { get; set; }
        public string DocNo { get; set; }
        public string ReminderList { get; set; }
        public DateTime DueDate { get; set; }
        public double AlertBeforeDays { get; set; }
        public string Attachment { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string Frequency { get; set; }
        public DateTime? OneTimeDate { get; set; }
        public TimeSpan ?Time { get; set; }
        public TimeSpan Time1 { get; set; }

        public double? Hours { get; set; }
        public TimeSpan DailyTime { get; set; }
        public List<string> WeeklyDays { get; set; }
        public string WeeklyDays1 { get; set; }
        public DateTime? MonthDate { get; set; }
        public DateTime ?QuarterlyDate { get; set; }
        public DateTime? YearlyDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status { get; set; }
        public string EmployeeId { get; set; }
    }
}