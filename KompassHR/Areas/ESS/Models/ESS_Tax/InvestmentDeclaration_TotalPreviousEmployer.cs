using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class InvestmentDeclaration_TotalPreviousEmployer
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
        public string InvestmentDeclarationEmployeeNo { get; set; }
        public string InvestmentDeclarationEmployeeName { get; set; }
        public float Previous_Er_GrossSalary { get; set; }
        public float Previous_Er_StandardDeduction { get; set; }
        public float Previous_Er_ProfessionalTax { get; set; }
        public float Previous_Er_80C { get; set; }
        public float Previous_Er_TaxDeductedAtSource { get; set; }
        public float Previous_Er_SelfOccupiedHousePropertyRepayment { get; set; }
        public float Previous_Er_Mediclaim80D { get; set; }
        public float Previous_Er_HandicappedDependent80DD { get; set; }
        public float Previous_Er_TreatmentOfSpecifiedDiseases80DDB { get; set; }
        public float Previous_Er_InterestOnEducationalLoan80E { get; set; }
        public float Previous_Er_Physicallyhandicapped80U { get; set; }
        public float Previous_Er_NPSEmployeeContribution80CCD1B { get; set; }
        public float Previous_Er_NPSEmployerContribution80CCD2 { get; set; }
    }
}