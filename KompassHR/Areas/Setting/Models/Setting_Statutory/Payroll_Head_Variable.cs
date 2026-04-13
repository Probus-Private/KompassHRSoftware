using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_Head_Variable
    {
        public double PayrollHeadVariableId { get; set; }
        public string PayrollHeadVariableId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public string SalaryEarn_RatePart_LeaveEncashment { get; set; }
        public bool SalaryEarn_RatePart_LeaveEncashment_IsApplicable { get; set; }
        public string SalaryEarn_RatePart_Arrears { get; set; }
        public bool SalaryEarn_RatePart_Arrears_IsApplicable { get; set; }
        public string SalaryEarn_RatePart_OverTime { get; set; }
        public bool SalaryEarn_RatePart_OverTime_IsApplicable { get; set; }
        public string SalaryEarn_RatePart_Incentive { get; set; }
        public bool SalaryEarn_RatePart_Incentive_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_A { get; set; }
        public bool SalaryEarn_RatePartM_A_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_B { get; set; }
        public bool SalaryEarn_RatePartM_B_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_C { get; set; }
        public bool SalaryEarn_RatePartM_C_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_D { get; set; }
        public bool SalaryEarn_RatePartM_D_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_E { get; set; }
        public bool SalaryEarn_RatePartM_E_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_F { get; set; }
        public bool SalaryEarn_RatePartM_F_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_G { get; set; }
        public bool SalaryEarn_RatePartM_G_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_H { get; set; }
        public bool SalaryEarn_RatePartM_H_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_I { get; set; }
        public bool SalaryEarn_RatePartM_I_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_J { get; set; }
        public bool SalaryEarn_RatePartM_J_IsApplicable { get; set; }
        public string SalaryDed_RatePart_Medical { get; set; }
        public bool SalaryDed_RatePart_Medical_IsApplicable { get; set; }
        public string SalaryDed_RatePart_FinePenalty { get; set; }
        public bool SalaryDed_RatePart_FinePenalty_IsApplicable { get; set; }
        public string SalaryDed_RatePart_Damages { get; set; }
        public bool SalaryDed_RatePart_Damages_IsApplicable { get; set; }
        public string SalaryDed_RatePart_TDS { get; set; }
        public bool SalaryDed_RatePart_TDS_IsApplicable { get; set; }
        public string SalaryDed_RatePart_Advance { get; set; }
        public bool SalaryDed_RatePart_Advance_IsApplicable { get; set; }
        public string SalaryDed_RatePart_Loan { get; set; }
        public bool SalaryDed_RatePart_Loan_IsApplicable { get; set; }
        public string SalaryDed_RatePart_Canteen { get; set; }
        public bool SalaryDed_RatePart_Canteen_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_A { get; set; }
        public bool SalaryDed_RatePartM_A_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_B { get; set; }
        public bool SalaryDed_RatePartM_B_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_C { get; set; }
        public bool SalaryDed_RatePartM_C_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_D { get; set; }
        public bool SalaryDed_RatePartM_D_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_E { get; set; }
        public bool SalaryDed_RatePartM_E_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_F { get; set; }
        public bool SalaryDed_RatePartM_F_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_G { get; set; }
        public bool SalaryDed_RatePartM_G_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_H { get; set; }
        public bool SalaryDed_RatePartM_H_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_I { get; set; }
        public bool SalaryDed_RatePartM_I_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_J { get; set; }
        public bool SalaryDed_RatePartM_J_IsApplicable { get; set; }
    }

    public class TypePayroll_Head_Variable
    {
        //PART A
        public double CmpID { get; set; }
        public string PayrollHeadVariableId_Encrypted { get; set; }
        public string SalaryEarn_RatePart_LeaveEncashment { get; set; }
        public bool SalaryEarn_RatePart_LeaveEncashment_IsApplicable { get; set; }
        public string SalaryEarn_RatePart_Arrears { get; set; }
        public bool SalaryEarn_RatePart_Arrears_IsApplicable { get; set; }
        public string SalaryEarn_RatePart_OverTime { get; set; }
        public bool SalaryEarn_RatePart_OverTime_IsApplicable { get; set; }
        public string SalaryEarn_RatePart_Incentive { get; set; }
        public bool SalaryEarn_RatePart_Incentive_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_A { get; set; }
        public bool SalaryEarn_RatePartM_A_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_B { get; set; }
        public bool SalaryEarn_RatePartM_B_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_C { get; set; }
        public bool SalaryEarn_RatePartM_C_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_D { get; set; }
        public bool SalaryEarn_RatePartM_D_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_E { get; set; }
        public bool SalaryEarn_RatePartM_E_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_F { get; set; }
        public bool SalaryEarn_RatePartM_F_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_G { get; set; }
        public bool SalaryEarn_RatePartM_G_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_H { get; set; }
        public bool SalaryEarn_RatePartM_H_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_I { get; set; }
        public bool SalaryEarn_RatePartM_I_IsApplicable { get; set; }
        public string SalaryEarn_RatePartM_J { get; set; }
        public bool SalaryEarn_RatePartM_J_IsApplicable { get; set; }

        //PART B
        public string SalaryDed_RatePart_Medical { get; set; }
        public bool SalaryDed_RatePart_Medical_IsApplicable { get; set; }
        public string SalaryDed_RatePart_FinePenalty { get; set; }
        public bool SalaryDed_RatePart_FinePenalty_IsApplicable { get; set; }
        public string SalaryDed_RatePart_Damages { get; set; }
        public bool SalaryDed_RatePart_Damages_IsApplicable { get; set; }
        public string SalaryDed_RatePart_TDS { get; set; }
        public bool SalaryDed_RatePart_TDS_IsApplicable { get; set; }
        public string SalaryDed_RatePart_Advance { get; set; }
        public bool SalaryDed_RatePart_Advance_IsApplicable { get; set; }
        public string SalaryDed_RatePart_Loan { get; set; }
        public bool SalaryDed_RatePart_Loan_IsApplicable { get; set; }
        public string SalaryDed_RatePart_Canteen { get; set; }
        public bool SalaryDed_RatePart_Canteen_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_A { get; set; }
        public bool SalaryDed_RatePartM_A_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_B { get; set; }
        public bool SalaryDed_RatePartM_B_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_C { get; set; }
        public bool SalaryDed_RatePartM_C_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_D { get; set; }
        public bool SalaryDed_RatePartM_D_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_E { get; set; }
        public bool SalaryDed_RatePartM_E_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_F { get; set; }
        public bool SalaryDed_RatePartM_F_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_G { get; set; }
        public bool SalaryDed_RatePartM_G_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_H { get; set; }
        public bool SalaryDed_RatePartM_H_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_I { get; set; }
        public bool SalaryDed_RatePartM_I_IsApplicable { get; set; }
        public string SalaryDed_RatePartM_J { get; set; }
        public bool SalaryDed_RatePartM_J_IsApplicable { get; set; }
    }
}