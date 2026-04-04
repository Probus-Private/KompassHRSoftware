using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Atten_MonthlyAttendance
    {
        public int EmployeeId { get; set; }
        public string AttenMonthlyId_Encrypted { get; set; }
        public int Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string MachineName { get; set; }
        public int AttenMonthlyCmpId { get; set; }
        public int AttenMonthlyBranchId { get; set; }
        public int AttenMonthlyEmployeeId { get; set; }
        public string AttenMonthlyMonthYear { get; set; }
        public float AttenMonthlyAB { get; set; }
        public float AttenMonthlyPP { get; set; }
        public float AttenMonthlyWO { get; set; }
        public float AttenMonthlyPH { get; set; }
        public float AttenMonthlyCO { get; set; }
        public float AttenMonthlyCL { get; set; }
        public float AttenMonthlySL { get; set; }
        public float AttenMonthlyEL { get; set; }
        public float AttenMonthlyML { get; set; }
        public float AttenMonthlyESIC { get; set; }
        public float AttenMonthlyLWP { get; set; }
        public float AttenMonthlyLL { get; set; }
        public float AttenMonthlyPayableDays { get; set; }
        public float AttenMonthlyLateMark { get; set; }
        public float AttenMonthlyOThrs { get; set; }
        public float AttenMonthlyLOPHrs { get; set; }
    }

    public class MonthlyAttendance
    {
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public DateTime Month { get; set; }
        public bool LoadWithLog { get; set; }
        public int ProcessCategoryId { get; set; }
    }

    public class BulkUploadMonthlyAttendance
    {
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveGroupId { get; set; }
        public string EmployeeName { get; set; }
        public int EmployeeNo { get; set; }
        public int EmployeeCardNo { get; set; }
        public string UnitName { get; set; }
        public int Mdays { get; set; }
        public int AA { get; set; }
        public int PP { get; set; }
        public int WO { get; set; }
        public int PH { get; set; }
        public int SL { get; set; }
        public int CL { get; set; }
        public int PL { get; set; }
        public int CO { get; set; }
        public int ML { get; set; }
        public int LWP { get; set; }
        public int OT { get; set; }
        public int LateMark { get; set; }
        public int EarlyGoing { get; set; }
    }

    public class DeleteMonthlyAttendance
    {
        public int AttenMonthlyId { get; set; }
    }

    public class ListMonthlyAttendance
    {
        public int AttenMonthlyBranchId { get; set; }
        public string BranchName { get; set; }
        public string CompanyName { get; set; }
        public string MonthYear { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime AttenMonthlyMonthYear { get; set; }
        public int Atten_ProcessCategoryId { get; set; }
        public string ProcessCategoryName { get; set; }
        public string ShortName { get; set; }
    }

    public class SalaryMonthlyAttendance
    {
        public int SalaryBranchId { get; set; }
        public string BranchName { get; set; }
        public string CompanyName { get; set; }
        public string MonthYear { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime SalaryMonthYear { get; set; }
        public int SalaryNetPay { get; set; }
        public int SalaryCmpId { get; set; }
        public string ProcessCategoryName { get; set; }
        public int SalaryProcessCategoryId { get; set; }
        public string ShortName { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingCount { get; set; }
        public string SalaryCheckerStatus { get; set; }
    }

    public class DeleteSalaryProcess
    {
        public string SalaryID { get; set; }
        public double SalaryEmployeeId { get; set; }
        public DateTime SalaryMonthYear { get; set; }
    }

    public class DeleteSalaryPayrollProcessViewModel
    {
        public List<DeleteSalaryProcess> ObjPayrollProcess { get; set; }

    }

    public class TypeAtten_MonthlyAttendance
    {
        public string HideColumns { get; set; }
        public string AttenMonthlyBranchId { get; set; }
        public string AttenMonthlyEmployeeId { get; set; }
        public string AttenMonthlyIsSalaryFullPay { get; set; }
        public string AttenMonthlyEmployeeLeft { get; set; }
        public string AttenMonthlyActualPayableDays { get; set; }
        public string ESIC { get; set; }
        public string PL { get; set; }
        public string OtherLeave { get; set; }
        public string LateComingCount { get; set; }
        public string LateHrs { get; set; }
        public string EarlyGoingCount { get; set; }
        public string EarlyHrs { get; set; }
        public string Atd1 { get; set; }
        public string Atd2 { get; set; }
        public string Atd3 { get; set; }
        public string TotalShortHrs { get; set; }
        public string DaysForLeaveDeduction { get; set; }
        public string Remark { get; set; }
        public string Active { get; set; }
        public string LeavingDate { get; set; }
        public string MonthDays { get; set; }
        public string PayableDays { get; set; }
        public string OTHrs { get; set; }
        public string AB { get; set; }
        public string PP { get; set; }
        public string WO { get; set; }
        public string PH { get; set; }
        public string CO { get; set; }
        public string CL { get; set; }
        public string SL { get; set; }
        public string EL { get; set; }
        public string ML { get; set; }
        public string LWP { get; set; }
        public string LOPDays { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string BusinessUnit { get; set; }
        public string DailyMonthly { get; set; }

        //public string  ObjMonthlyAttendance { get; set; }
    }


    public class SaveMonthlyAttendanceViewModel
    {
        public List<TypeAtten_MonthlyAttendance> ObjMonthlyAttendance { get; set; }
     
    }


    public class TypeAtten_DeleteMonthlyAttendance
    {
        public string AttenMonthlyId { get; set; }
        public string AttenMonthlyEmployeeId { get; set; }
        public DateTime AttenMonthlyMonthYear { get; set; }
    }

    public class DeleteMonthlyAttendanceViewModel
    {
        public List<TypeAtten_DeleteMonthlyAttendance> ObjMonthlyAttendance { get; set; }

    }

    public class RecordList
    {
        public int SalaryID { get; set; }
        public int SalaryEmployeeId { get; set; }
        public DateTime SalaryMonthYear { get; set; }
        public int SalaryCmpId { get; set; }
        public int SalaryBranchId { get; set; }
        public string Remark { get; set; }
        public string Status { get; set; }
    }
    public class ApproveRejectPayrollSalaryRequestViewModel
    {
        public List<RecordList> ObjPayrollsalary { get; set; }

    }
}

