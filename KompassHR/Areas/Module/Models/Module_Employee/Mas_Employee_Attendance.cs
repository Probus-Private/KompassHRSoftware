using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Attendance
    {
        public double AttendanceId { get; set; }
        public string AttendanceId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double AttendanceEmployeeId { get; set; }
        public string EmployeeCardNo { get; set; }
        public bool? EM_Atten_OT_Applicable { get; set; }
        public float EM_Atten_OTMultiplyBy { get; set; }
        public float EM_Atten_PerDayShiftHrs { get; set; }
        public bool? EM_Atten_CoffApplicable { get; set; }
        public double EM_Atten_CoffSettingId { get; set; }
        public double EM_Atten_ShiftGroupId { get; set; }
        public double EM_Atten_ShiftRuleId { get; set; }
        public string EM_Atten_WOFF1 { get; set; }
        public string EM_Atten_WOFF2 { get; set; }
        public string EM_Atten_WOFF2_ForCheck { get; set; }
        public bool EM_Atten_WOFF_Check1 { get; set; }
        public bool EM_Atten_WOFF_Check2 { get; set; }
        public bool EM_Atten_WOFF_Check3 { get; set; }
        public bool EM_Atten_WOFF_Check4 { get; set; }
        public bool EM_Atten_WOFF_Check5 { get; set; }
        public bool EM_Atten_LateMarkSettingApplicable { get; set; }
        public int EM_Atten_LateMarkSettingId { get; set; }
        public bool? EM_Atten_IsSalaryFullPay { get; set; }
        public double EM_Atten_LeaveGroupId { get; set; }
        public bool? EM_Atten_DefaultAttenShow { get; set; }
        public bool EM_Atten_SinglePunch_Present { get; set; }
        public bool EM_Atten_RotationalWeekOff { get; set; }
        public bool? EM_Atten_Regularization_Required { get; set; }
        public bool? EM_Atten_ShortLeaveApplicable { get; set; }
        public double EM_Atten_ShortLeaveSettingId { get; set; }
        public bool? EM_Atten_PersonalGatepassApplicable { get; set; }
        public double EM_Atten_Atten_PersonalGatepassSettingId { get; set; }
        public DateTime EM_Atten_AttendanceLastDate { get; set; }
        public bool EM_Atten_PunchMissingApplicable { get; set; }
        public double EM_Atten_Atten_PunchMissingSettingId { get; set; }
        public bool EM_Atten_flexibleShiftApplicable { get; set; }
        public bool PHApplicable { get; set; }
        public double EM_Atten_Atten_OutDoorCompanySettingId { get; set; }
        public bool EM_Atten_OutDoorCompanyApplicable { get; set; }
        public bool EM_Atten_WOPH_CoffApplicable { get; set; }
        public List<dynamic> CheckedList { get; set; }
        public bool EM_Atten_ShortHRS_Applicable { get; set; }
        //public string EM_Atten_DailyMonthly { get; set; }
        public int EM_Atten_LocationRegistrationMappingIMasterId { get; set; }
        public bool EM_Atten_MonthlyRosterApplicable { get; set; }
        public bool EM_Atten_CanteenApplicable { get; set; }
        public string EM_Atten_LocationRegistrationId { get; set; }
        public string SelectedLocations { get; set; }
        public bool EM_Atten_IsLocationRemarkCompulsory { get; set; }

    }
}