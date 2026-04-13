using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Models
{
    public class Mas_Employee
    {
        public int EmployeeId { get; set; }
        public string EmployeeId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double EmployeeBranchId { get; set; }
        public string EmployeeOrigin { get; set; }
        public string EmployeeSeries { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public bool IsNRI { get; set; }
        public string Salutation { get; set; }
        public string EmployeeName { get; set; }
        public double EmployeeLevelID { get; set; }
        public double EmployeeWageID { get; set; }
        public double EmployeeDepartmentID { get; set; }
        public double EmployeeSubDepartmentName { get; set; }
        public double EmployeeDesignationID { get; set; }
        public double EmployeeGradeID { get; set; }
        public double EmployeeGroupID { get; set; }
        public double EmployeeTypeID { get; set; }
        public double EmployeeCostCenterID { get; set; }
        public double EmployeeZoneID { get; set; }
        public double EmployeeUnitID { get; set; }
        public double ContractorID { get; set; }
        public double EmployeeLineID { get; set; }
        public DateTime? JoiningDate { get; set; }
        public bool IsJoiningSpecial { get; set; }
        public string JoiningStatus { get; set; }
        public DateTime? TraineeDueDate { get; set; }
        public DateTime? ProbationDueDate { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public bool IsConfirmation { get; set; }
        public string ConfirmationBy { get; set; }
        public string CompanyMobileNo { get; set; }
        public string CompanyMailID { get; set; }
        public double ReportingHR { get; set; }
        public double ReportingManager1 { get; set; }
        public double ReportingManager2 { get; set; }
        public double ReportingAccount { get; set; }
        public double NoOfBranchTransfer { get; set; }
        public bool IsReasigned { get; set; }
        public DateTime? ResignationDate { get; set; }
        public int NoticePeriodDays { get; set; }
        public bool IsExit { get; set; }
        public DateTime? ExitDate { get; set; }
        public bool EmployeeLeft { get; set; }
        public DateTime? LeavingDate { get; set; }
        public string LeavingReason { get; set; }
        public string LeavingReasonPF { get; set; }
        public string LeavingReasonESI { get; set; }
        public DateTime? EM_PayrollLastDate { get; set; }
        public string LocalExpat { get; set; }
        public double DocNo { get; set; }
        public DateTime? DocDate { get; set; }
        public string FID_Encrypted { get; set; }
        public double PreboardingFid { get; set; }


        public string HiddenEmployeeNo { get; set; }
        public string HiddenEmployeeSeries { get; set; }

        public bool? IsCriticalStageApplicable { get; set; }
        public double EmployeeCriticalStageID { get; set; }
        public double EmployeeAllocationCategoryId { get; set; }
        public string EM_Atten_DailyMonthly { get; set; }
        public string NoticePeriodType { get; set; }
        public int NoticePeriodMonthDays { get; set; }

        public int EM_ProcessCategoryId { get; set; }
        public int EM_SalarySheetId { get; set; }

        public int EmployeeCountryId { get; set; }

    }
}