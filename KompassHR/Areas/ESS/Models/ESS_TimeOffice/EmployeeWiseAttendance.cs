using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class EmployeeWiseAttendance
    {
        public int? EmployeeID { get; set; }
        public int? BranchId { get; set; }
        public int? CmpId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool OTApproved { get; set; }
        public bool PunchAdjustment { get; set; }
    }


    public class GetAttendance
    {
        public DateTime InOut_AttendanceDate { get; set; }
        public int InOut_EmployeeId { get; set; }
        public DateTime? InOut_InTime { get; set; }
        public DateTime? InOut_OutTime { get; set; }
        public string InOut_inoutnote { get; set; }
        public bool isRestricted { get; set; }
    }

    public class GetAttendanceMultiEmp
    {
        public DateTime InOut_AttendanceDate { get; set; }
        public string InOut_EmployeeNo { get; set; }
        public DateTime? InOut_InTime { get; set; }
        public DateTime? InOut_OutTime { get; set; }
        public string InOut_inoutnote { get; set; }
    }

    public class EmployeeWiseAttendanceList
    {
        public DateTime Date { get; set; }
        public TimeSpan InTime { get; set; }
        public TimeSpan OutTime { get; set; }
        public string Shift { get; set; }
        public string Status { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan OverTime { get; set; }
        public int LateComing { get; set; }
        public TimeSpan LateMark { get; set; }
        public TimeSpan EarlyGoing { get; set; }
        public int NoOffCoff { get; set; }
        public string Remark { get; set; }
    }

    public class EmployeeWiseAttendanceReg
    {
        public string EmployeeId { get; set; }
        public string Date { get; set; }
        public string Day { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string Status { get; set; }
        //public string ShiftId { get; set; }
        public string Shift { get; set; }
        public string TotalDuration { get; set; }
        public string OverTime { get; set; }
        // public string LateMark { get; set; }
        public string LateComing { get; set; }
        public string EarlyGoing { get; set; }
        public string NoOffCOFF { get; set; }
        public string Remark { get; set; }
    }

    public class AttendanceRegu_Atten_InOut
    {
        public int InOutID { get; set; }
        public string InOutID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int InOutBranchId { get; set; }
        public int InOutEmployeeId { get; set; }
        public DateTime InOutMonthYear { get; set; }
        public DateTime InOutDate { get; set; }
        public DateTime InOutIntime { get; set; }
        public DateTime InOutOutTime { get; set; }
        public DateTime InOutIntimeS { get; set; }
        public DateTime InOutOutTimeS { get; set; }
        public double InOutDuration { get; set; }
        public double InOutDurationS { get; set; }
        public int InOutLateMark { get; set; }
        public double InOutLateBy { get; set; }
        public int InOutEarlyMark { get; set; }
        public double InOutEarlyBy { get; set; }
        public double InOutOT_End { get; set; }
        public double InOutOT_Start { get; set; }
        public double InOutCoff { get; set; }
        public bool InOutCoffApprove { get; set; }
        public DateTime InOutCoffDate { get; set; }
        public string InOutCoffApproveStatus { get; set; }
        public int InOutCoffApproveRejectBy { get; set; }
        public string InOutShift { get; set; }
        public string InOutStatus { get; set; }
        public string InOutSubStatus { get; set; }
        public string InOutNote { get; set; }
        public int InOutLeaveID { get; set; }
        public bool InOutAdj { get; set; }
        public string InOutAdjRemark { get; set; }
        public int InOutShiftId { get; set; }
        public int InOutRuleId { get; set; }
        public string InOutRegStatus { get; set; }
        public string InOutRegInDevice { get; set; }
        public string InOutRegOutDevice { get; set; }
        public string InOutRegNote { get; set; }
        public string RequestFrom { get; set; }
        public string InOut_WO { get; set; }
        public string InOut_OT_Coff { get; set; }
        public string InOut_Rule_ShiftGroup { get; set; }
        public int InOut_EmpDepartmentId { get; set; }
        public int InOut_EmpContractorId { get; set; }
        public int InOut_EmpDesignationId { get; set; }
        public int InOut_EmpCategoryId { get; set; }
        public int InOut_EmpGradeId { get; set; }
        public bool InOut_EmpCriticalY_N { get; set; }
        public int InOut_EmpCriticalSatgeId { get; set; }
        public int InOut_EmpLevelId { get; set; }
        public int InOut_EmpLineId { get; set; }
        public int InOut_EmpUnitId { get; set; }
        public int InOutShiftDuration { get; set; }
        public string InOut_EmpGender { get; set; }
        public double InOut_PP { get; set; }
        public double InOut_AB { get; set; }
        public double InOut_WeekOff { get; set; }
        public double InOut_PH { get; set; }
        public double InOut_CL { get; set; }
        public double InOut_SL { get; set; }
        public double InOut_EL { get; set; }
        public double InOut_ML { get; set; }
        public double InOut_CO { get; set; }
        public double InOut_LWP { get; set; }
        public double InOut_ESIC { get; set; }
        public string Remark { get; set; }
        public DateTime InOutFromDate { get; set; }
        public DateTime InOutToDate { get; set; }

    }
}