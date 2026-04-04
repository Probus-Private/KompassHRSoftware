using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class InvestmentDeclaration_80C
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
        public double US80C_ProvidentFund_CurrentAmt { get; set; }
        public double US80C_ProvidentFund_LimitAmt { get; set; }
        public double US80C_ProvidentFund_EligibleDeductionAmt { get; set; }
        public double US80C_VoluntaryProvidentFund_CurrentAmt { get; set; }
        public double US80C_VoluntaryProvidentFund_LimitAmt { get; set; }
        public double US80C_VoluntaryProvidentFund_EligibleDeductionAmt { get; set; }
        public double US80C_EmployeesNationalPensionScheme80CCD1_CurrentAmt { get; set; }
        public double US80C_EmployeesNationalPensionScheme80CCD1_LimitAmt { get; set; }
        public double US80C_EmployeesNationalPensionScheme80CCD1_EligibleDeductionAmt { get; set; }
        public double US80C_PensionPlanfromInsuranceCompaniesMutualFunds_CurrentAmt { get; set; }
        public double US80C_PensionPlanfromInsuranceCompaniesMutualFunds_LimitAmt { get; set; }
        public double US80C_PensionPlanfromInsuranceCompaniesMutualFunds_EligibleDeductionAmt { get; set; }
        public double US80C_HousingLoanPrincipalRepayment_CurrentAmt { get; set; }
        public double US80C_HousingLoanPrincipalRepayment_LimitAmt { get; set; }
        public double US80C_HousingLoanPrincipalRepayment_EligibleDeductionAmt { get; set; }
        public double US80C_LifeInsurancePremium_CurrentAmt { get; set; }
        public double US80C_LifeInsurancePremium_LimitAmt { get; set; }
        public double US80C_LifeInsurancePremium_EligibleDeductionAmt { get; set; }
        public double US80C_NationalSavingCertificate_CurrentAmt { get; set; }
        public double US80C_NationalSavingCertificate_LimitAmt { get; set; }
        public double US80C_NationalSavingCertificate_EligibleDeductionAmt { get; set; }
        public double US80C_NationalSavingCertificateAccruedInterest_CurrentAmt { get; set; }
        public double US80C_NationalSavingCertificateAccruedInterest_LimitAmt { get; set; }
        public double US80C_NationalSavingCertificateAccruedInterest_EligibleDeductionAmt { get; set; }
        public double US80C_PublicProvidentFund_CurrentAmt { get; set; }
        public double US80C_PublicProvidentFund_LimitAmt { get; set; }
        public double US80C_PublicProvidentFund_EligibleDeductionAmt { get; set; }
        public double US80C_UnitLinkedInsurancePlan_CurrentAmt { get; set; }
        public double US80C_UnitLinkedInsurancePlan_LimitAmt { get; set; }
        public double US80C_UnitLinkedInsurancePlan_EligibleDeductionAmt { get; set; }
        public double US80C_ELSSTaxSavingMutualFund_CurrentAmt { get; set; }
        public double US80C_ELSSTaxSavingMutualFund_LimitAmt { get; set; }
        public double US80C_ELSSTaxSavingMutualFund_EligibleDeductionAmt { get; set; }
        public double US80C_Tuitionfeesfor2children_CurrentAmt { get; set; }
        public double US80C_Tuitionfeesfor2children_LimitAmt { get; set; }
        public double US80C_Tuitionfeesfor2children_EligibleDeductionAmt { get; set; }
        public double US80C_TaxSavingSharesNABARDandOtherBonds_CurrentAmt { get; set; }
        public double US80C_TaxSavingSharesNABARDandOtherBonds_LimitAmt { get; set; }
        public double US80C_TaxSavingSharesNABARDandOtherBonds_EligibleDeductionAmt { get; set; }
        public double US80C_TaxsavingFixDepositsTenure5yearsormore_CurrentAmt { get; set; }
        public double US80C_TaxsavingFixDepositsTenure5yearsormore_LimitAmt { get; set; }
        public double US80C_TaxsavingFixDepositsTenure5yearsormore_EligibleDeductionAmt { get; set; }
        public double US80C_HousingStampDutyRegistration_CurrentAmt { get; set; }
        public double US80C_HousingStampDutyRegistration_LimitAmt { get; set; }
        public double US80C_HousingStampDutyRegistration_EligibleDeductionAmt { get; set; }
        public double US80C_PostOfficeTimeDepositScheme_CurrentAmt { get; set; }
        public double US80C_PostOfficeTimeDepositScheme_LimitAmt { get; set; }
        public double US80C_PostOfficeTimeDepositScheme_EligibleDeductionAmt { get; set; }
        public double US80C_SeniorCitizenSavingScheme_CurrentAmt { get; set; }
        public double US80C_SeniorCitizenSavingScheme_LimitAmt { get; set; }
        public double US80C_SeniorCitizenSavingScheme_EligibleDeductionAmt { get; set; }
        public double US80C_SukanyaSamriddhiScheme_CurrentAmt { get; set; }
        public double US80C_SukanyaSamriddhiScheme_LimitAmt { get; set; }
        public double US80C_SukanyaSamriddhiScheme_EligibleDeductionAmt { get; set; }
        public double US80C_80CCC_CurrentAmt { get; set; }
        public double US80C_80CCC_LimitAmt { get; set; }
        public double US80C_80CCC_EligibleDeductionAmt { get; set; }
        public double US80C_SuperannuationFundContribution_CurrentAmt { get; set; }
        public double US80C_SuperannuationFundContribution_LimitAmt { get; set; }
        public double US80C_SuperannuationFundContribution_EligibleDeductionAmt { get; set; }
        public double US80C_OtherInvestment_CurrentAmt { get; set; }
        public double US80C_OtherInvestment_LimitAmt { get; set; }
        public double US80C_OtherInvestment_EligibleDeductionAmt { get; set; }
        public double US80C_OtherInvestment1_CurrentAmt { get; set; }
        public double US80C_OtherInvestment1_LimitAmt { get; set; }
        public double US80C_OtherInvestment1_EligibleDeductionAmt { get; set; }
        public double US80C_OtherInvestment2_CurrentAmt { get; set; }
        public double US80C_OtherInvestment2_LimitAmt { get; set; }
        public double US80C_OtherInvestment2_EligibleDeductionAmt { get; set; }
        public double US80C_OtherInvestment3_CurrentAmt { get; set; }
        public double US80C_OtherInvestment3_LimitAmt { get; set; }
        public double US80C_OtherInvestment3_EligibleDeductionAmt { get; set; }
        public double US80C_Total { get; set; }
        public double US80C_Total_Limit { get; set; }
        public double US80C_Total_EligibleDeduction { get; set; }

        public string US80C_HouseLoanPrincipalRepayment_ProofUpload { get; set; }
        public string FilePath { get; set; }
    }
}