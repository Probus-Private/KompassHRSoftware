using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class Mas_Employee_CTC_Template
    {
        public double TemplateCompanyId { get; set; }
        public string TemplateName { get; set; }

        //public double RateOTRate { get; set; }
        //public double CategoryId { get; set; }
        //public double RatePartA_Basic { get; set; }
        //public double RatePartA_DA { get; set; }
        //public double RatePartA_HRA { get; set; }
        //public double RatePartA_A { get; set; }
        //public double RatePartA_B { get; set; }
        //public double RatePartA_C { get; set; }
        //public double RatePartA_D { get; set; }
        //public double RatePartA_E { get; set; }
        //public double RatePartA_F { get; set; }
        //public double RatePartA_G { get; set; }
        //public double RatePartA_H { get; set; }
        //public double RatePartA_I { get; set; }
        //public double RatePartA_J { get; set; }
        //public double RatePartA_K { get; set; }
        //public double RatePartA_L { get; set; }
        //public double RatePartA_M { get; set; }
        //public double RatePartA_N { get; set; }
        //public double RatePartA_O { get; set; }
        //public double RatePartA_P { get; set; }
        //public double RatePartA_Q { get; set; }
        //public double RatePartA_R { get; set; }
        //public double RatePartA_S { get; set; }
        //public double RatePartA_T { get; set; }
        //public double RateTotalPartA { get; set; }
        //public double RatePartB_PFEmployer { get; set; }
        //public double RatePartB_ESICEmployer { get; set; }
        //public double RatePartB_LWFEmployer { get; set; }
        //public double RatePartB_Bonus { get; set; }
        //public double RatePartB_Gratuity { get; set; }
        //public double RatePartB_A { get; set; }
        //public double RatePartB_B { get; set; }
        //public double RatePartB_C { get; set; }
        //public double RatePartB_D { get; set; }
        //public double RatePartB_E { get; set; }
        //public double RateTotalPartB { get; set; }
        //public double RateTotalPartAB { get; set; }
        //public double Rate_VPF { get; set; }
        //public double RateBudgetId { get; set; }

        //public double RatePartB_PFEmployee { get; set; }
        //public double RatePartB_ESICEmployee { get; set; }
        //public double RatePartB_PTEmployee { get; set; }
        //public double Rate_NetPay { get; set; }
        //public double RatePartB_AttendanceBonus { get; set; }
    }

    public class TypeMas_Employee_CTC_PartA
    {
        public double RatePartA_Basic { get; set; }
        public double RatePartA_DA { get; set; }
        public double RatePartA_HRA { get; set; }
        public double RatePartA_A { get; set; }
        public double RatePartA_B { get; set; }
        public double RatePartA_C { get; set; }
        public double RatePartA_D { get; set; }
        public double RatePartA_E { get; set; }
        public double RatePartA_F { get; set; }
        public double RatePartA_G { get; set; }
        public double RatePartA_H { get; set; }
        public double RatePartA_I { get; set; }
        public double RatePartA_J { get; set; }
        public double RatePartA_K { get; set; }
        public double RatePartA_L { get; set; }
        public double RatePartA_M { get; set; }
        public double RatePartA_N { get; set; }
        public double RatePartA_O { get; set; }
        public double RatePartA_P { get; set; }
        public double RatePartA_Q { get; set; }
        public double RatePartA_R { get; set; }
        public double RatePartA_S { get; set; }
        public double RatePartA_T { get; set; }
        public double RateTotalPartA { get; set; }
    }

    public class TypeMas_Employee_CTC_PartB
    {
        public double RatePartB_PFEmployer { get; set; }
        public double RatePartB_ESICEmployer { get; set; }
        public double RatePartB_LWFEmployer { get; set; }
        public double RatePartB_Bonus { get; set; }
        public double RatePartB_Gratuity { get; set; }
        public double RatePartB_A { get; set; }
        public double RatePartB_B { get; set; }
        public double RatePartB_C { get; set; }
        public double RatePartB_D { get; set; }
        public double RatePartB_E { get; set; }
        public double RateTotalPartB { get; set; }
        public double RateTotalPartAB { get; set; }
    }
}