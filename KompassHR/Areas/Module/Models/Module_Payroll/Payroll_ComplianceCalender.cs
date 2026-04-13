using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_ComplianceCalender
    {
        public int ComplianceCalenderId { get; set; }
        public string ComplianceCalenderId_Encrypted { get; set; }
        public int Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public string ComplianceName { get; set; }
        public int ComplianceCategoryId { get; set; }
          public string Authority { get; set; }
        //   public List<int> Authority { get; set; }

        public string EmployeeId { get; set; }
        public List<int> EmployeeIds { get; set; }

        public string Frequency { get; set; }
        public DateTime ?DueDate { get; set; }
        public double Reminder { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }

        public DateTime? OneTimeDate { get; set; }
        public TimeSpan? Time { get; set; }
        public TimeSpan Time1 { get; set; }
        public double? Hours { get; set; }
        public TimeSpan DailyTime { get; set; }
        public List<string> WeeklyDays { get; set; }
        public string WeeklyDays1 { get; set; }
        //public DateTime? MonthDate { get; set; }
        public Double MonthDate { get; set; }
        // public DateTime? QuarterlyDate { get; set; }
        public string QuarterlyDate { get; set; }
        public DateTime? YearlyDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Attachment { get; set; }
       // public Dictionary<string, DateTime?> QuarterlyDateList { get; set; }
        public Dictionary<string, string> QuarterlyDateList { get; set; }


    }
}