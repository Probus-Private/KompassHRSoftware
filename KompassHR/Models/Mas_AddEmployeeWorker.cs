using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Models
{
    public class Mas_AddEmployeeWorker
    {

        public string EmployeeId_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double EmployeeId { get; set; }
        public string EmployeeBranchId { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public string Salutation { get; set; }
        public string EmployeeName { get; set; }
        public double ContractorID { get; set; }
        public double EmployeeDepartmentID { get; set; }
        public double EmployeeSubDepartmentName { get; set; }
        public double EmployeeDesignationID { get; set; }
        public double EmployeeGradeID { get; set; }
        public double EmployeeUnitID { get; set; }
        public double EmployeeLevelID { get; set; }
        public bool? IsCriticalStageApplicable { get; set; }
        public double EmployeeCriticalStageID { get; set; }
        public double EmployeeLineID { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        public bool EmployeeLeft { get; set; }

        public string PersonalId_Encrypted { get; set; }
        public double PersonalEmployeeId { get; set; }
        public string AadhaarNo { get; set; }
        public string PrimaryMobile { get; set; }
        public DateTime BirthdayDate { get; set; }
        public double EmployeeQualificationID { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string NameAsPerAadhaar { get; set; }

        public string AddressId_Encrypted { get; set; }
        public double AddressEmployeeId { get; set; }
        public string PresentPin { get; set; }
        public string PresentState { get; set; }
        public string PresentDistrict { get; set; }
        public string PresentTaluka { get; set; }
        public string PresentPO { get; set; }
        public string PresentCity { get; set; }
        public string PresentPostelAddress { get; set; }

        public string AttendanceId_Encrypted { get; set; }
        public double AttendanceEmployeeId { get; set; }
        public string EM_Atten_WOFF1 { get; set; }
        public double EM_Atten_ShiftGroupId { get; set; }
        public double EM_Atten_ShiftRuleId { get; set; }
        public bool? EM_Atten_OT_Applicable { get; set; }
        public float EM_Atten_OTMultiplyBy { get; set; }
        public float EM_Atten_PerDayShiftHrs { get; set; }
        public bool PHApplicable { get; set; }
        public double EmployeeAllocationCategoryId { get; set; }

        public string PF_FSType { get; set; }
        public string PF_FS_Name { get; set; }
        public string ESIC_NO { get; set; }
        public string PF_UAN { get; set; }
        public bool IsAttendacnce { get; set; }
        public string EM_Atten_DailyMonthly { get; set; }
        public bool EM_Atten_CanteenApplicable { get; set; }

    }
}