using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Models
{
    public class EmployeeGlobalSearch
    {
        public int EmployeeId { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeLeft { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string SubDepartment { get; set; }
        public string DesignationName { get; set; }
        public string GradeName { get; set; }
        public string Contractor { get; set; }
        public string EmployeeGroup { get; set; }
        public string EmployeeType { get; set; }
        public string EmployeeLevel { get; set; }
        public string WageCategory { get; set; }
        public string CostCenter { get; set; }
        public string Unit { get; set; }
        public string CriticalStageApplicable { get; set; }
        public string CriticalStage { get; set; }
        public string LineName { get; set; }
        public string JoiningDate { get; set; }
        public string ConfirmationDate { get; set; }
        public string LeavingDate { get; set; }
        public string OT { get; set; }
        public string CoffApplicable { get; set; }
        public string WOFF1 { get; set; }
        public string WOFF2 { get; set; }
        public string AadharNo { get; set; }
        public string PanNo { get; set; }
        public string PrimaryMobileNo { get; set; }
        public string PrimaryMobile { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string PersonalEmailId { get; set; }
    }

    public class EmployeeHistory
    {
        public string ShortName { get; set; }
        public string BranchName { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public string ContractorName { get; set; }
        public string JoiningDate { get; set; }
        public string Status { get; set; }
        public string LeavingDate { get; set; }
        public string Duration { get; set; }
        public string AadhaarNo { get; set; }
        public string BirthdayDate { get; set; }
        public string Gender { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }

    }
}