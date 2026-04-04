using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Employee_EmployeeTransfer_Premium
    {
        public double EmployeeTransferId { get; set; }
        public string EmployeeTransferId_Encrypted { get; set; }
        public double CmpId { get; set; }
        public double EmployeeId { get; set; }
        public string BranchId { get; set; }
        public string ToBranchId { get; set; }
        public string EmployeeName { get; set; }
       public bool AttendanceBonusApplicable { get; set; }
        public bool CanteenApplicable { get; set; }
        public DateTime TransferDate { get; set; }
        public string DailyMonthly { get; set; }
        public string ShiftGroup { get; set; }
        public string ShiftRule { get; set; }
    }

    public class CanteenApplicableModel
    {
        public bool IsCanteenApplicable { get; set; }
        public bool IsAttenBonusApplicable { get; set; }
        public string DailyMonthly { get; set; }
    }
}