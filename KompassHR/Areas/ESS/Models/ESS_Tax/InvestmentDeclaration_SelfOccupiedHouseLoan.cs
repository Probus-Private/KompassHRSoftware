using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Controllers.ESS_Tax
{
    public class InvestmentDeclaration_SelfOccupiedHouseLoan
    {
        public int InvestmentDeclarationId { get; set; }
        public string InvestmentDeclaration_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int InvestmentDeclarationFyearId { get; set; }
        public int InvestmentDeclarationEmployeeId { get; set; }
        public double SelfOccupied_TypeID { get; set; }
        public string SelfOccupied_Type { get; set; }
        public int SelfOccupied_CurrentAmt { get; set; }
        public decimal SelfOccupied_Limit { get; set; }
        public decimal SelfOccupied_EligibleDeductionAmt { get; set; }
        public bool SelfOccupied_Submit { get; set; }
        public DateTime SelfOccupied_SubmitDate { get; set; }
        public double SelfOccupied_SubmitCount { get; set; }
        public double SelfOccupied_TotalAmount { get; set; }
        public bool SelfOccupied_IsCoOwner { get; set; }
        public double SelfOccupied_Co_Applicant_Percentage { get; set; }
        public string SelfOccupied_ProofUpload { get; set; }
    }
}