using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Payroll
{
    public class Payroll_BulkCTCInsert
    {
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double CategoryId { get; set; }
        public double SalaryBankTranfer { get; set; }
        public double BudgetId { get; set; }
        public double SalaryModeId { get; set; }
    }

    public class BulkCTCInsert_Class
    {

        public string EmployeeCTCId { get; set; }
        public string EmployeeCTCId_Encrypted { get; set; }
        public string Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string CTCEmployeeId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string IsIncrement { get; set; }
        public string DailyMonthly { get; set; }
        public string RateOTRate { get; set; }
        public string CategoryId { get; set; }
        public string RatePartA_Basic { get; set; }
        public string RatePartA_DA { get; set; }
        public string RatePartA_HRA { get; set; }
        public string RatePartA_A { get; set; }
        public string RatePartA_B { get; set; }
        public string RatePartA_C { get; set; }
        public string RatePartA_D { get; set; }
        public string RatePartA_E { get; set; }
        public string RatePartA_F { get; set; }
        public string RatePartA_G { get; set; }
        public string RatePartA_H { get; set; }
        public string RatePartA_I { get; set; }
        public string RatePartA_J { get; set; }
        public string RatePartA_K { get; set; }
        public string RatePartA_L { get; set; }
        public string RatePartA_M { get; set; }
        public string RatePartA_N { get; set; }
        public string RatePartA_O { get; set; }
        public string RatePartA_P { get; set; }
        public string RatePartA_Q { get; set; }
        public string RatePartA_R { get; set; }
        public string RatePartA_S { get; set; }
        public string RatePartA_T { get; set; }
        public string RateTotalPartA { get; set; }
        public string RatePartB_PFEmployer { get; set; }
        public string RatePartB_ESICEmployer { get; set; }
        public string RatePartB_LWFEmployer { get; set; }
        public string RatePartB_Bonus { get; set; }
        public string RatePartB_Gratuity { get; set; }
        public string RatePartB_A { get; set; }
        public string RatePartB_B { get; set; }
        public string RatePartB_C { get; set; }
        public string RatePartB_D { get; set; }
        public string RatePartB_E { get; set; }
        public string RateTotalPartB { get; set; }
        public string RateTotalPartAB { get; set; }
        public string NextIncrementDate { get; set; }
        public string Remark { get; set; }
        public string Rate_VPF { get; set; }
        public string RateSalaryMode { get; set; }
        public string RateMakerEmployeeId { get; set; }
        public string RateCheckerEmployeeId { get; set; }
        public string RateCheckerDateAndTime { get; set; }
        public string RateCmpBankId { get; set; }
        public string RateBudgetId { get; set; }
    }
}