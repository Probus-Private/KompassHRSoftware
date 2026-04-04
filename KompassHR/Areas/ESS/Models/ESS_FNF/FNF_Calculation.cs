using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_FNF
{
    public class FNF_Calculation
    {
        public double FNFCalId { get; set; }
        public string FNFCal_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }

        //Leave Calculation
        public string LeaveMasterLeaveYearId { get; set; }

        //FNF Details
        public string DocDate { get; set; }
        public string DocNo { get; set; }
        public string FNFCal_CmpId { get; set; }
        public string FNFCal_BranchId { get; set; }
        public string FNFCal_ReasonId { get; set; }
        public string FNFCal_ReasonRemark { get; set; }
        public string FNFCal_EmployeeId { get; set; }
        public string CalculationDate { get; set; }
        public string PaidMonthYear { get; set; }
        public string FNFCal_EmployeeName { get; set; }
        public string FNFCal_EmployeNo { get; set; }
        public string joiningDate { get; set; }
        public string FNFCal_ResignationDate { get; set; }
        public string LastWorkingDate { get; set; }
        public string FNFCal_TotalExperance { get; set; }
        public string FNFCal_DepartmentNameId { get; set; }
        public string departmentname { get; set; }
        public string FNFCal_DesignationId { get; set; }
        public string designationname { get; set; }
        public string FNFCal_GradeId { get; set; }
        public string gradename { get; set; }
        public string AadhaarNo { get; set; }
        public string PAN { get; set; }
        public string PF_NO { get; set; }
        public string ESIC_NO { get; set; }
        public string PF_UAN { get; set; }
        public string ResignationDate { get; set; }
        public string ReasonName { get; set; }
        public string ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public string ApproveRejectDate { get; set; }
        public string NoticePeriod { get; set; }
        public string TotalLeaveEncashment { get; set; }
        public string BonusPreviousFromDate { get; set; }
        public string BonusPreviousToDate { get; set; }
        public string TotalBonusPreviousAmt { get; set; }
        public string BonusCurrentFromDate { get; set; }
        public string BonusCurrentToDate { get; set; }
        public string TotalBonusCurrentAmt { get; set; }
        public string DailyMonthly { get; set; }
        public string PayableDays { get; set; }
        public string OTHrs { get; set; }
        public string Notice_Pay_Type { get; set; }
        public string Notice_Pay_Days { get; set; }
        public string Notice_Purchase_Type { get; set; }
        public string Notice_Purchase_Days { get; set; }
        public string ExtraLeaveRecoverdDays { get; set; }
        public string CompanyName { get; set; }
        public string BusinessUnit { get; set; }
        public string WorkDuration { get; set; }
        public string PrimaryMobile { get; set; }
        public string LeaveEncashment { get; set; }
        public string ExtraLeaveRecovered { get; set; }
        public string Arrears { get; set; }
        public string OtherEarningRemark1 { get; set; }
        public string OtherEarning1 { get; set; }
        public string OtherEarningRemark2 { get; set; }
        public string OtherEarning2 { get; set; }
        public string OtherEarningRemark3 { get; set; }
        public string OtherEarning3 { get; set; }

        public string OtherDeductionRemark1 { get; set; }
        public string OtherDeduction1 { get; set; }
        public string OtherDeductionRemark2 { get; set; }
        public string OtherDeduction2 { get; set; }
        public string OtherDeductionRemark3 { get; set; }
        public string OtherDeduction3 { get; set; }

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
    }
    public class FNF_GratuityCalculation
    {
        public double GratuityCalculationId { get; set; }
        public string GratuityCalculationId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }

        public string CurrentBasic_DA { get; set; }
        public string GratuityYear { get; set; }
        public string RountOf_GratuityYear { get; set; }
        public string GratuityAmount { get; set; }
        public string CauseOfExit { get; set; }
    }
}