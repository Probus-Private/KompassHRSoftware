using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_FNF
{
    public class Maxroof_FNFCalculations
    {
        public double FNFCald { get; set; }
        public string FNFCal_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double FNFCal_EmployeeId { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public double DocNo { get; set; }
        public DateTime? FNFDate { get; set; }

        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Grade { get; set; }
        public string BusinessUnit { get; set; }
        public DateTime? JoiningDate { get; set; }
        public double ResignationId { get; set; }
        public DateTime? ResignationDate { get; set; }
        public DateTime? LastWorkingDate { get; set; }
        public DateTime? RelievingDate { get; set; }
        public string DailyMonthly { get; set; }
        public DateTime? NoticePeriodEndDate { get; set; }
        public string NoticePeriod { get; set; }
        public string Bond_Applicable { get; set; }
        public DateTime? BondEndDate { get; set; }
        public string BondAmt { get; set; }

        public string A_Salary_Applicable { get; set; } = "No";
        public DateTime? A_SalaryDate { get; set; }
        public string B_Salary_Applicable { get; set; } = "No";
        public DateTime? B_SalaryDate { get; set; }
        public string C_Salary_Applicable { get; set; } = "No";
        public DateTime? C_SalaryDate { get; set; }

        public string FY_Preivous_Bonus_Year_Applicable { get; set; } = "No";
        public double FY_Preivous_Bonus_Year { get; set; }

        public string FY_Current_Bonus_Year_Applicable { get; set; } = "No";
        public double FY_Current_Bonus_Year { get; set; }

        public string LeaveYear_Applicable { get; set; } = "No";
        public double LeaveMasterLeaveYearId { get; set; }

        public string ServiceBond_Applicable { get; set; }

        public string UnpaidSalaryA_Date { get; set; }
        public string UnpaidSalaryB_Date { get; set; }
        public string UnpaidSalaryC_Date { get; set; }

        public string UnpaidSalaryA_Amount { get; set; }
        public string LastSalaryPaidMonth { get; set; }
        public string PLDays { get; set; }
        public string PayableDays { get; set; }
        public string MonthDays { get; set; }
        public string LOPDays { get; set; }
        public string EffectiveWorkDays { get; set; }

        public string UnpaidSalaryB_Amount { get; set; }
        public string UnpaidSalaryC_Amount { get; set; }
        public string TotalUnpaidSalaryAmount { get; set; }

        public string PreivousBonus_Remark { get; set; }
        public string BonusPreviousYearAmount { get; set; }
        public string CurrentBonus_Remark { get; set; }
        public string BonusCurrentYearAmount { get; set; }
        public string TotalBonus_Amount { get; set; }

        public string LeavePreviousYear { get; set; }
        public string PreivousLeave_Days { get; set; }
        public string LeavePreviousYearAmount { get; set; }
        public string LeaveCurrentYear { get; set; }
        public string CurrentLeave_Days { get; set; }
        public string LeaveCurrentYearAmount { get; set; }
        public string TotalLeave_Amount { get; set; }

        public string TotalIncome { get; set; }

        public string AdvanceRecovered { get; set; }
        public string SalaryTDS { get; set; }

        public string TotalDeduction { get; set; }

        public string NetPay { get; set; }
        public string NetPayInWord { get; set; }
        public string Gratuity_Applicable { get; set; }
        public string Gratuity_Service { get; set; }
        public string GratuityAmount { get; set; }
        public string GratuityAmountInWord { get; set; }
        public string Remark { get; set; }
        public string FinalNetPayable { get; set; }
        public string FinalNetPayableInWord { get; set; }

        public List<RecordList> RecordList { get; set; }
        public List<FNF_Earning_Info> earnings { get; set; }
        public List<FNF_Deduction_Info> deductions { get; set; }
    }

    public class RecordList
    {
        public string FNFCald { get; set; }
        public string Status { get; set; }
    }

    public class FNF_Deduction_Info
    {
        public string Head { get; set; }
        public string DisplayHead { get; set; }
        public string DeductionAmt { get; set; }
    }

    public class FNF_Earning_Info
    {
        public string Head { get; set; }
        public string DisplayHead { get; set; }
        public string EarningAmt { get; set; }
    }
}