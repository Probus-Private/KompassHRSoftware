using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_InvestmentDeclaration_PreviousEmployer
    {
        public int PreviousEmployerId { get; set; }
        public string PreviousEmployerId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int PreviousEmployerFyearId { get; set; }
        public int PreviousEmployerEmployeeId { get; set; }
        public string PreviousEmployerName { get; set; }
        public string PreviousEmployerAddress { get; set; }
        public float PreviousEmployerGrossSalary { get; set; }
        public float PreviousEmployerStandardDeduction { get; set; }
        public float PreviousEmployerProfessionalTax { get; set; }
        public float PreviousEmployer80C { get; set; }
        public float PreviousEmployerTaxDeductedAtSource { get; set; }
        public float PreviousEmployerSelfOccupiedHousePropertyRepayment { get; set; }
        public float PreviousEmployerMediclaim80D { get; set; }
        public float PreviousEmployerHandicappedDependent80DD { get; set; }
        public float PreviousEmployerTreatmentOfSpecifiedDiseases80DDB { get; set; }
        public float PreviousEmployerInterestOnEducationalLoan80E { get; set; }
        public float PreviousEmployerPhysicallyhandicapped80U { get; set; }
        public float PreviousEmployerNPSEmployeeContribution80CCD1B { get; set; }
        public float PreviousEmployerNPSEmployerContribution80CCD2 { get; set; }
        public bool Previous_Er_Unemployed { get; set; }
        public string Previous_Er_UnemployedDescription { get; set; }

        public string Previous_Er_ProofUpload { get; set; }
        public string FilePath { get; set; }

        public DateTime PreviousEmployerJoiningDate { get; set; }
        public DateTime PreviousEmployerLeavingDate { get; set; }
        public DateTime PreviousEmployerSalaryMonth { get; set; }
    }
}