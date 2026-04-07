using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Reports.Models
{
    public class ReportFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    //Use for all Reports
    public class DailyAttendanceReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
        public string Origin { get; set; }
        public int? DepartmentId { get; set; }
        public int? DesignationId { get; set; }
        public int? LineId { get; set; }
        public int? ContractorId { get; set; }
        public int? GradeId { get; set; }
        public int? SubDepartmentId { get; set; }
        public int? EmployeeID { get; set; }
        public bool CheckBox { get; set; }
        public int? NoOfDays { get; set; }
        public string LeaveYear { get; set; } //LeaveSummary
        public string Type { get; set; }   // WeeklyOff
        public int? GrievanceCategoryId { get; set; }
        public int? GrievanceSubCategoryId { get; set; }
        public string ShiftSortName { get; set; }
        public int? MinHours { get; set; }
        public int? MaxHours { get; set; }
        public int? SubUnitId { get; set; }
        public int? MinNoOfDays { get; set; }
        public int? MaxNoOfDays { get; set; }
        public int? ModuleId { get; set; }
        public string Gender { get; set; }
        public string AadharNo { get; set; }
        public double ProjectID { get; set; }
        public double ClientID { get; set; }
        public double AssignToEmployeeID { get; set; }
        public string Status { get; set; }
    }

    public class Contractor_InvoiceReport
    {
        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
        public int? ContractorId { get; set; }
        public DateTime InvoiceMonth { get; set; }
    }

    public class Atten_LogNotFound
    {
        public int? CmpId { get; set; }
        public string EmployeeCardNo { get; set; }
    }

    //This is remove
    public class MonthWiseReportFilter
    {
        public DateTime Month { get; set; }
    }

    public class MonthWiseFilter
    {
        public DateTime? Month { get; set; }
        //This below 3 for temparory use
        public int? CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public int StateId { get; set; }
        public int PayrollActId { get; set; }

        
    }

    public class WeekOffAdjustFilter
    {

        public int? CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public string WeekOffType { get; set; }
        public int WeekOffAdjustDate { get; set; }
    }

    public class MusterAttendanceActualAttendance
    {
        public int? CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime MonthYear { get; set; }

    }

    public class KIPLCheckInOut
    {
        public int? CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

    }

    public class EmployeeWiseReportFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public int AssetId { get; set; }
        public bool CheckBox { get; set; }
        public String Status { get; set; }
    }
    public class YearlyWiseLoanReportFilter
    {
        public int FromDate { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }

    }

    public class EmployeeWiseLoanReportFilter
    {
        public int FromDate { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
    }

    public class LeaveReportFilter
    {
        public int YearId { get; set; }
        public int LeaveId { get; set; }
        public int EmployeeId { get; set; }
    }

    public class ActualLogReportFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public bool CheckBox { get; set; }
    }
    public class WeeklyOff
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Type { get; set; }
        public bool CheckBox { get; set; }
    }
    public class NewJoiningEmployeeReport
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double Department { get; set; }
    }

    public class RptLeaveSummery
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        // public double LeaveType { get; set; }
        public double LeaveYear { get; set; }
        public int EmployeeId { get; set; }
        public bool CheckAll { get; set; }
        public double LeaveTypeId { get; set; }
    }

    public class Merged_MonthlySummary
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public int LeaveYearId { get; set; }
        public int EmployeeId { get; set; }
        public double LeaveTypeId { get; set; }
    }

    public class ShiftDropDown
    {
        public string Name { get; set; }
        public string SName { get; set; }

    }

    public class HeadCountSummary
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public bool CheckAll { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string GradeId { get; set; }
        public DateTime Date { get; set; }
        public Nullable<System.TimeSpan> FromTime { get; set; }
        public Nullable<System.TimeSpan> ToTime { get; set; }
    }

    public class CustomizeReport
    {
        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
        public int? SubDepartmentId { get; set; }
        public int? DesignationId { get; set; }
        public int? GradeId { get; set; }
        public int? LineId { get; set; }
        public int? EmployeeID { get; set; }
        public int? Active_Deactive { get; set; }
    }


    public class LeaveWithWage
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public int? Year { get; set; }
        public int? EmployeeId { get; set; }
    }

    public class Payroll_Atten_Info
    {
        public string AttenType { get; set; }
        public string Total { get; set; }
    }

    public class Payroll_Deduction_Info
    {
        public string Head { get; set; }
        public string DeductionAmt { get; set; }
    }

    public class Payroll_Earning_Info
    {
        public string Head { get; set; }
        public string StandardAmt { get; set; }
        public string EarningAmt { get; set; }
        public string ArreasAmt { get; set; }
    }
    public class ResignationWidrawal
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public DateTime? FromMonth { get; set; }
        public DateTime? ToMonth { get; set; }
        public double EmployeeId { get; set; }
    }
    public class BranchWiseSummary
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public DateTime? FromMonth { get; set; }
        public DateTime? ToMonth { get; set; }
        public double Department { get; set; }
        public int? Percentage { get; set; }
    }
    public class PaidBonus
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public int FinantialYear { get; set; }
    }
    public class CTCMaster
    {
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public bool Statutory { get; set; }
        public DateTime PayrollDate { get; set; }
    }

    public class PayrollApproval
    {
        public DateTime? Month { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public bool Type { get; set; }
    }

    public class EmployeeConfirmation
    {
        public DateTime? Month { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
    }

    public class Rpt_Atten_EmployeeWiseSummery
    {
        public string PresentDays { get; set; }
        public string WeekOffDays { get; set; }
        public string PaidHolidays { get; set; }
        public string AbsentDays { get; set; }
        public string Overtime { get; set; }
        public string CO { get; set; }
        public string CasualLeave { get; set; }
        public string SickLeave { get; set; }
        public string EarnedLeave { get; set; }
        public string MaternityLeave { get; set; }
        public string ESICLeave { get; set; }
        public string LWPLeave { get; set; }
        public string PhysicalWorkedHours { get; set; }
        public string WorkedonWoff { get; set; }
        public string LOPHrs { get; set; }
        public string PayableDays { get; set; }
        public string MonthDays { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string BusinessUnit { get; set; }
        public string DepartmentName { get; set; }
        public string DesginationName { get; set; }
    }

    public class EmployeeWiseSalarySlip
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string EmployeeNo { get; set; }
    }

    public class HeadWise
    {
        public DateTime? Month { get; set; }
        //This below 3 for temparory use
        public int? CmpId { get; set; }
        public int branchId { get; set; }
    }

    public class TaxFilters
    {
        public int FinancialYear { get; set; }
        public string TType { get; set; }
        public int RegimeTypeID { get; set; }
        public DateTime MonthYear { get; set; }
        public string TAN { get; set; }
        public string Quarter { get; set; }
    }

    public class TentativeTaxCalculation
    {
        public int CmpId { get; set; }
        public string TType { get; set; }
        public DateTime MonthYear { get; set; }
    }

    public class IncomeTaxData
    {
        public int CmpId { get; set; }
        public int FinantialYear { get; set; }
        public string IncomeTaxType { get; set; }
    }


    public class AgencyDetailsReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
        public int? DepartmentId { get; set; }
        public int? DesignationId { get; set; }
        public double AgencyId { get; set; }
    }

    public class ShortListedAndRejectedCandidateReport
    {
        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
        public string ResumeStatus { get; set; }
    }

    public class PostInterviewStatusReport
    {

        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
        public string InterviewStatus { get; set; }

    }

    public class EmployeeAgeBond
    {
        public int? CmpId { get; set; }
        public int BranchId { get; set; }
        public double Difference { get; set; }
    }

    public class DuplicateDetails
    {
        public int? CmpId { get; set; }
        public int BranchId { get; set; }
        public string Duplicate { get; set; }
        public double Type { get; set; }
    }
    public class ExpiryNotification
    {
        public int? CmpId { get; set; }
        public int BranchId { get; set; }
        public string Type { get; set; }
        public DateTime MonthYear { get; set; }
        public bool IsAll { get; set; } = false;
    }
    public class Manpower_ContractorWiseRateRpt
    {
        public int? CmpId { get; set; }
        public int BranchId { get; set; }
        public int ContractorId { get; set; }
        public DateTime FromMonth { get; set; }
        public DateTime ToMonth { get; set; }
    }
    public class NotLogin
    {
        public int day { get; set; }
    }
    public class Atten_MonthlyAttendance
    {
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public DateTime Month { get; set; }
        public int ProcessCategoryId { get; set; }
    }

    public class FNFFilter
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public DateTime? FromMonth { get; set; }
        public DateTime? ToMonth { get; set; }
        public string ResignationType { get; set; }
    }
    public class TMSFilter
    {
        public double ClientId { get; set; }
        public double ProjectId { get; set; }
        public int EmployeeId { get; set; }
        public Double ModuleId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    public class BonusLTAFilter
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public DateTime? FromMonth { get; set; }
        public DateTime? ToMonth { get; set; }
        public string Head { get; set; }
    }
}