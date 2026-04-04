using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_CTC
    {
        public double EmployeeCTCId { get; set; }
        public string EmployeeCTCId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CTCEmployeeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsIncrement { get; set; }
        public string NextIncrementDate { get; set; }
        public string Remark { get; set; }
        public string DailyMonthly { get; set; }

        public string RateSalaryMode { get; set; }
        public double RateMakerEmployeeId { get; set; }
        public double RateCheckerEmployeeId { get; set; }
        public DateTime RateCheckerDateAndTime { get; set; }
        public double RateCmpBankId { get; set; }

        public double RateOTRate { get; set; }
        public double CategoryId { get; set; }
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
        public double Rate_VPF { get; set; }
        public double RateBudgetId { get; set; }

        public double TemplateId { get; set; }

        public double RatePartB_PFEmployee { get; set; }
        public double RatePartB_ESICEmployee { get; set; }
        public double RatePartB_PTEmployee { get; set; }
        public double Rate_NetPay { get; set; }
        public double RatePartB_AttendanceBonus { get; set; }

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

    public class RateHeadVM
    {
        public int Seq { get; set; }
        public string ID { get; set; }
        public string Heads { get; set; }
    }
}