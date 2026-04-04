using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class CTCHeadMapping
    {
        public double PayrollHeadId { get; set; }
        public string PayrollHeadId_Encrypted { get; set; }
        public string Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PayrollMap_CompanyId { get; set; }

        //partial A
        public string RatePartA_Basic { get; set; }
        public bool RatePartA_Basic_IsApplicable { get; set; }
        public string RatePartA_DA { get; set; }
        public bool RatePartA_DA_IsApplicable { get; set; }
        public string RatePartA_HRA { get; set; }
        public bool RatePartA_HRA_IsApplicable { get; set; }
        public string RatePartA_A { get; set; }
        public bool RatePartA_A_IsApplicable { get; set; }
        public string RatePartA_B { get; set; }
        public bool RatePartA_B_IsApplicable { get; set; }
        public string RatePartA_C { get; set; }
        public bool RatePartA_C_IsApplicable { get; set; }
        public string RatePartA_D { get; set; }
        public bool RatePartA_D_IsApplicable { get; set; }
        public string RatePartA_E { get; set; }
        public bool RatePartA_E_IsApplicable { get; set; }
        public string RatePartA_F { get; set; }
        public bool RatePartA_F_IsApplicable { get; set; }
        public string RatePartA_G { get; set; }
        public bool RatePartA_G_IsApplicable { get; set; }
        public string RatePartA_H { get; set; }
        public bool RatePartA_H_IsApplicable { get; set; }
        public string RatePartA_I { get; set; }
        public bool RatePartA_I_IsApplicable { get; set; }
        public string RatePartA_J { get; set; }
        public bool RatePartA_J_IsApplicable { get; set; }
        public string RatePartA_K { get; set; }
        public bool RatePartA_K_IsApplicable { get; set; }
        public string RatePartA_L { get; set; }
        public bool RatePartA_L_IsApplicable { get; set; }
        public string RatePartA_M { get; set; }
        public bool RatePartA_M_IsApplicable { get; set; }
        public string RatePartA_N { get; set; }
        public bool RatePartA_N_IsApplicable { get; set; }
        public string RatePartA_O { get; set; }
        public bool RatePartA_O_IsApplicable { get; set; }
        public string RatePartA_P { get; set; }
        public bool RatePartA_P_IsApplicable { get; set; }
        public string RatePartA_Q { get; set; }
        public bool RatePartA_Q_IsApplicable { get; set; }
        public string RatePartA_R { get; set; }
        public bool RatePartA_R_IsApplicable { get; set; }
        public string RatePartA_S { get; set; }
        public bool RatePartA_S_IsApplicable { get; set; }
        public string RatePartA_T { get; set; }
        public bool RatePartA_T_IsApplicable { get; set; }

        //partial A
        public string RatePartB_PFEmployer { get; set; }
        public bool RatePartB_PFEmployer_IsApplicable { get; set; }
        public string RatePartB_ESICEmployer { get; set; }
        public bool RatePartB_ESICEmployer_IsApplicable { get; set; }
        public string RatePartB_LWFEmployer { get; set; }
        public bool RatePartB_LWFEmployer_IsApplicable { get; set; }
        public string RatePartB_Bonus { get; set; }
        public bool RatePartB_Bonus_IsApplicable { get; set; }
        public string RatePartB_Gratuity { get; set; }
        public bool RatePartB_Gratuity_IsApplicable { get; set; }
        public string RatePartB_A { get; set; }
        public bool RatePartB_A_IsApplicable { get; set; }
        public string RatePartB_B { get; set; }
        public bool RatePartB_B_IsApplicable { get; set; }
        public string RatePartB_C { get; set; }
        public bool RatePartB_C_IsApplicable { get; set; }
        public string RatePartB_D { get; set; }
        public bool RatePartB_D_IsApplicable { get; set; }
        public string RatePartB_E { get; set; }
        public bool RatePartB_E_IsApplicable { get; set; }
    }

    public class TypeCTCHeadMapping
    {
        public string PayrollHeadId_Encrypted { get; set; }
        public double PayrollMap_CompanyId { get; set; }
        public string RatePartA_Basic { get; set; }
        public bool RatePartA_Basic_IsApplicable { get; set; }
        public string RatePartA_DA { get; set; }
        public bool RatePartA_DA_IsApplicable { get; set; }
        public string RatePartA_HRA { get; set; }
        public bool RatePartA_HRA_IsApplicable { get; set; }
        public string RatePartA_A { get; set; }
        public bool RatePartA_A_IsApplicable { get; set; }
        public string RatePartA_B { get; set; }
        public bool RatePartA_B_IsApplicable { get; set; }
        public string RatePartA_C { get; set; }
        public bool RatePartA_C_IsApplicable { get; set; }
        public string RatePartA_D { get; set; }
        public bool RatePartA_D_IsApplicable { get; set; }
        public string RatePartA_E { get; set; }
        public bool RatePartA_E_IsApplicable { get; set; }
        public string RatePartA_F { get; set; }
        public bool RatePartA_F_IsApplicable { get; set; }
        public string RatePartA_G { get; set; }
        public bool RatePartA_G_IsApplicable { get; set; }
        public string RatePartA_H { get; set; }
        public bool RatePartA_H_IsApplicable { get; set; }
        public string RatePartA_I { get; set; }
        public bool RatePartA_I_IsApplicable { get; set; }
        public string RatePartA_J { get; set; }
        public bool RatePartA_J_IsApplicable { get; set; }
        public string RatePartA_K { get; set; }
        public bool RatePartA_K_IsApplicable { get; set; }
        public string RatePartA_L { get; set; }
        public bool RatePartA_L_IsApplicable { get; set; }
        public string RatePartA_M { get; set; }
        public bool RatePartA_M_IsApplicable { get; set; }
        public string RatePartA_N { get; set; }
        public bool RatePartA_N_IsApplicable { get; set; }
        public string RatePartA_O { get; set; }
        public bool RatePartA_O_IsApplicable { get; set; }
        public string RatePartA_P { get; set; }
        public bool RatePartA_P_IsApplicable { get; set; }
        public string RatePartA_Q { get; set; }
        public bool RatePartA_Q_IsApplicable { get; set; }
        public string RatePartA_R { get; set; }
        public bool RatePartA_R_IsApplicable { get; set; }
        public string RatePartA_S { get; set; }
        public bool RatePartA_S_IsApplicable { get; set; }
        public string RatePartA_T { get; set; }
        public bool RatePartA_T_IsApplicable { get; set; }

        //partial A
        public string RatePartB_PFEmployer { get; set; }
        public bool RatePartB_PFEmployer_IsApplicable { get; set; }
        public string RatePartB_ESICEmployer { get; set; }
        public bool RatePartB_ESICEmployer_IsApplicable { get; set; }
        public string RatePartB_LWFEmployer { get; set; }
        public bool RatePartB_LWFEmployer_IsApplicable { get; set; }
        public string RatePartB_Bonus { get; set; }
        public bool RatePartB_Bonus_IsApplicable { get; set; }
        public string RatePartB_Gratuity { get; set; }
        public bool RatePartB_Gratuity_IsApplicable { get; set; }
        public string RatePartB_A { get; set; }
        public bool RatePartB_A_IsApplicable { get; set; }
        public string RatePartB_B { get; set; }
        public bool RatePartB_B_IsApplicable { get; set; }
        public string RatePartB_C { get; set; }
        public bool RatePartB_C_IsApplicable { get; set; }
        public string RatePartB_D { get; set; }
        public bool RatePartB_D_IsApplicable { get; set; }
        public string RatePartB_E { get; set; }
        public bool RatePartB_E_IsApplicable { get; set; }
    }


}