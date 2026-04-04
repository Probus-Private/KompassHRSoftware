using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Leave
{
    public class Leave_OpeningBalance
    {
        public double LeaveOpeningBalanceId { get; set; }
        public string LeaveOpeningBalanceId_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double LeaveOpeningBalanceLeaveYearId { get; set; }
        public double LeaveOpeningBalanceSettingId { get; set; }
        public double LeaveOpeningBalanceLeaveGroupId { get; set; }
        public double LeaveOpeningBalanceLeaveTypeId { get; set; }
        public double LeaveOpeningBalanceEmployeeId { get; set; }
        public float NoOfLeave { get; set; }
        public string OpeningAdjCarryCredit_Remark { get; set; }
        public double OpeningAdjCarryCredit_DocId { get; set; }
        public double LeaveOpeningBalanceBranchID { get; set; }
        public string Remark { get; set; }
    }
    public class LeaveOpeningBalance
    {
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string DesignationName { get; set; }
        public string BusinessUnit { get; set; }
        public string DepartmentName { get; set; }
        public string JoiningDate { get; set; }
        public string LeaveType { get; set; }
        public string YearlyLeave { get; set; }
        public string LeaveYear { get; set; }

    }

    public class LeaveMonthlyCredit
    {
        public int CmpId { get; set; }
        public int LeaveSettingId { get; set; }
        public DateTime? MonthYear { get; set; }
    }

    public class LeaveTypeIdName
    {
        public string Id { get; set; }
        public int LeaveTypeId { get; set; }
        public string Name { get; set; }
    }

    public class CoffMonthlyCredit
    {
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public DateTime? MonthYear { get; set; }
    }
    public class CoffMonthlyCreditList
    {
        public int EmployeeId { get; set; }
        public double ApprovedCoff { get; set; }
        public double NoOfCoff { get; set; }
    }
}