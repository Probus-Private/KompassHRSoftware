using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_FNF
{
    public class SM_FNFCalculations
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
        public string Desination { get; set; }
        public string Grade { get; set; }
        public string BusinessUnit { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? ReginationDate { get; set; }
        public DateTime? LastWorkingDate { get; set; }
        public DateTime? RelievingDate { get; set; }
        public string DailyMonthly { get; set; }
        public DateTime? NoticePeriodEndDate { get; set; }
        public string NoticePeriod { get; set; }
        public string Bond_Applicable { get; set; }
        public DateTime? BondEndDate { get; set; }
        public string BondAmt { get; set; }
        //public string BondRemark { get; set; }

        public string GrossSalary { get; set; }
        public string BasicSalary { get; set; }
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
        public string UnpaidSalaryB_Amount { get; set; }
        public string UnpaidSalaryC_Amount { get; set; }
        public string TotalUnpaidSalaryAmount { get; set; }

        public string PreivousBonus_Remark { get; set; }
        public string BonusPreviousYearAmount { get; set; }
        public string CurrentBonus_Remark { get; set; }
        public string BonusCurrentYearAmount { get; set; }
        public string TotalBonus_Amount { get; set; }

        public string OtherEarningA_Amount { get; set; }
        public string OtherEarningA_Remark { get; set; }

        public string OtherEarningB_Amount { get; set; }
        public string OtherEarningB_Remark { get; set; }
        public string TotalOtherEarning { get; set; }

        public string LeavePreviousYear { get; set; }
        public string PreivousLeave_Days { get; set; }
        public string LeavePreviousYearAmount { get; set; }
        public string LeaveCurrentYear { get; set; }
        public string CurrentLeave_Days { get; set; }
        public string LeaveCurrentYearAmount { get; set; }

        public string OtherDeductionA_Remark { get; set; }
        public string OtherDeductionA_Amount { get; set; }

        public string OtherDeductionB_Remark { get; set; }
        public string OtherDeductionB_Amount { get; set; }

        public string TotalLeaveEncashmentAmount { get; set; }
        public string TotalAmountPayable { get; set; }
        public string LessDeduction { get; set; }
        public string AdvanceRecovered { get; set; }
        public string SalaryTDS { get; set; }
        public string NoticePeriodDays { get; set; }
        public string NoticePeriodDayAmount { get; set; }
        public string BondRemark { get; set; }
        public string MinServiceBondDeduction { get; set; }
        public string NoticePeriodRemark { get; set; }
        public string OtherAmount { get; set; }
        public string TotalAmountDeduction { get; set; }
        public string NetAmountPayable { get; set; }
        public string NetAmountPayableInWords { get; set; }

        public string Gratuity_Applicable { get; set; }
        public string Gratuity_Service { get; set; }

        public string Remark { get; set; }
        public double ResignationId { get; set; }
    }
}