using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class InvestmentDeclaration_Approval
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
        public string HRA_Apr_Metro { get; set; }
        public string HRA_May_Metro { get; set; }
        public string HRA_Jun_Metro { get; set; }
        public string HRA_Jul_Metro { get; set; }
        public string HRA_Aug_Metro { get; set; }
        public string HRA_Sep_Metro { get; set; }
        public string HRA_Oct_Metro { get; set; }
        public string HRA_Nov_Metro { get; set; }
        public string HRA_Dec_Metro { get; set; }
        public string HRA_Jan_Metro { get; set; }
        public string HRA_Feb_Metro { get; set; }
        public string HRA_Mar_Metro { get; set; }
        public string HRA_Apr_CityName { get; set; }
        public string HRA_May_CityName { get; set; }
        public string HRA_Jun_CityName { get; set; }
        public string HRA_Jul_CityName { get; set; }
        public string HRA_Aug_CityName { get; set; }
        public string HRA_Sep_CityName { get; set; }
        public string HRA_Oct_CityName { get; set; }
        public string HRA_Nov_CityName { get; set; }
        public string HRA_Dec_CityName { get; set; }
        public string HRA_Jan_CityName { get; set; }
        public string HRA_Feb_CityName { get; set; }
        public string HRA_Mar_CityName { get; set; }
        public string HRA_Apr_LandlordName { get; set; }
        public string HRA_May_LandlordName { get; set; }
        public string HRA_Jun_LandlordName { get; set; }
        public string HRA_Jul_LandlordName { get; set; }
        public string HRA_Aug_LandlordName { get; set; }
        public string HRA_Sep_LandlordName { get; set; }
        public string HRA_Oct_LandlordName { get; set; }
        public string HRA_Nov_LandlordName { get; set; }
        public string HRA_Dec_LandlordName { get; set; }
        public string HRA_Jan_LandlordName { get; set; }
        public string HRA_Feb_LandlordName { get; set; }
        public string HRA_Mar_LandlordName { get; set; }
        public string HRA_Apr_PanNo { get; set; }
        public string HRA_May_PanNo { get; set; }
        public string HRA_Jun_PanNo { get; set; }
        public string HRA_Jul_PanNo { get; set; }
        public string HRA_Aug_PanNo { get; set; }
        public string HRA_Sep_PanNo { get; set; }
        public string HRA_Oct_PanNo { get; set; }
        public string HRA_Nov_PanNo { get; set; }
        public string HRA_Dec_PanNo { get; set; }
        public string HRA_Jan_PanNo { get; set; }
        public string HRA_Feb_PanNo { get; set; }
        public string HRA_Mar_PanNo { get; set; }
        public string HRA_Apr_Address { get; set; }
        public string HRA_May_Address { get; set; }
        public string HRA_Jun_Address { get; set; }
        public string HRA_Jul_Address { get; set; }
        public string HRA_Aug_Address { get; set; }
        public string HRA_Sep_Address { get; set; }
        public string HRA_Oct_Address { get; set; }
        public string HRA_Nov_Address { get; set; }
        public string HRA_Dec_Address { get; set; }
        public string HRA_Jan_Address { get; set; }
        public string HRA_Feb_Address { get; set; }
        public string HRA_Mar_Address { get; set; }
        public double HRA_Apr_CurrentAmt { get; set; }
        public double HRA_May_CurrentAmt { get; set; }
        public double HRA_Jun_CurrentAmt { get; set; }
        public double HRA_Jul_CurrentAmt { get; set; }
        public double HRA_Aug_CurrentAmt { get; set; }
        public double HRA_Sep_CurrentAmt { get; set; }
        public double HRA_Oct_CurrentAmt { get; set; }
        public double HRA_Nov_CurrentAmt { get; set; }
        public double HRA_Dec_CurrentAmt { get; set; }
        public double HRA_Jan_CurrentAmt { get; set; }
        public double HRA_Feb_CurrentAmt { get; set; }
        public double HRA_Mar_CurrentAmt { get; set; }
        public double HRA_Total { get; set; }

        public string HRAProof_Upload { get; set; }


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
        public DateTime US80C_SubmitDate { get; set; }
        public double US80C_SubmitCount { get; set; }


        public double SelfOccupied_TypeID { get; set; }
        public string SelfOccupied_Type { get; set; }
        public double SelfOccupied_CurrentAmt { get; set; }
        public decimal SelfOccupied_Limit { get; set; }
        public decimal SelfOccupied_EligibleDeductionAmt { get; set; }
        public bool SelfOccupied_Submit { get; set; }
        public DateTime SelfOccupied_SubmitDate { get; set; }
        public double SelfOccupied_SubmitCount { get; set; }
        public double SelfOccupied_TotalAmount { get; set; }
        public bool SelfOccupied_IsCoOwner { get; set; }

        public double IncomeFromLetOutHousePropertyId { get; set; }
        public string IncomeFromLetOutHousePropertyId_Encrypted { get; set; }
        public double IncomeFromLetOutHousePropertyFyearId { get; set; }
        public double IncomeFromLetOutHousePropertyEmployeeId { get; set; }
        public string DescriptionOfProperty { get; set; }
        public float GrossAnnualValue { get; set; }
        public float MunicipalTaxes { get; set; }
        public float NetAnnualValue { get; set; }
        public float StandardDeduction { get; set; }
        public float InterestOnBorrowedCapital { get; set; }
        public float IncomeFromLetOutHouseProperty { get; set; }
        public double TotalIncomeFromLetOutHouseProperty { get; set; }

        public bool ChapterVIA_Medical80D_IsSelfCoverd { get; set; }
        public double ChapterVIA_Medical80D_Self_CurrentAmt { get; set; }
        public double ChapterVIA_Medical80D_Self_LimitAmt { get; set; }
        public double ChapterVIA_Medical80D_Self_EligibleDeductionAmt { get; set; }
        public bool ChapterVIA_Medical80D_IsAreYourParentCovered { get; set; }
        public bool ChapterVIA_Medical80D_IsParentSeniorCitizen { get; set; }
        public double ChapterVIA_Medical80D_Parents_CurrentAmt { get; set; }
        public double ChapterVIA_Medical80D_Parents_LimitAmt { get; set; }
        public double ChapterVIA_Medical80D_Parents_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_Medical80D_HealthCheckup_CurrentAmt { get; set; }
        public double ChapterVIA_Medical80D_HealthCheckup_LimitAmt { get; set; }
        public double ChapterVIA_Medical80D_HealthCheckup_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_Medical80D_FinalEligibleDeduction { get; set; }
        public bool ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent { get; set; }
        public bool ChapterVIA_HandicappedDependent80DD_IsSevere { get; set; }
        public double ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent_CurrentAmt { get; set; }
        public double ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent_LimitAmt { get; set; }
        public double ChapterVIA_HandicappedDependent80DD_IsHandicappedDependent_EligibleDeductionAmt { get; set; }
        public bool ChapterVIA_TreatmentofSpecifiedDiseases80DDB_IsSeniorCitizen { get; set; }
        public double ChapterVIA_TreatmentofSpecifiedDiseases80DDB_CurrentAmt { get; set; }
        public double ChapterVIA_TreatmentofSpecifiedDiseases80DDB_LimitAmt { get; set; }
        public double ChapterVIA_TreatmentofSpecifiedDiseases80DDB_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_TreatmentofSpecifiedDiseases80DDB_AmountRecoveredFromInsuranceCompany_CurrentAmt { get; set; }
        public bool ChapterVIA_InterestOnEducationalLoan80E_IsFullTime { get; set; }
        public double ChapterVIA_InterestOnEducationalLoan80E_CurrentAmt { get; set; }
        //Missing Elegibility name = ChapterVIA_InterestOnEducationalLoan80E_EligibleAmt
        public bool ChapterVIA_PhysicallyHandicapped80U_IsPhysicallyHandicapped { get; set; }
        public bool ChapterVIA_PhysicallyHandicapped80U_IsSevere { get; set; }
        public double ChapterVIA_PhysicallyHandicapped80U_LimitAmt { get; set; }
        public double ChapterVIA_PhysicallyHandicapped80U_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_AdditionalcontributionToNPS80CCD1B_CurrentAmt { get; set; }
        public double ChapterVIA_AdditionalcontributionToNPS80CCD1B_LimitAmt { get; set; }
        public double ChapterVIA_AdditionalcontributionToNPS80CCD1B_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_InterestondepositionSavingaccount80TTA_CurrentAmt { get; set; }
        public double ChapterVIA_InterestondepositionSavingaccount80TTA_LimitAmt { get; set; }
        public double ChapterVIA_InterestondepositionSavingaccount80TTA_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_InterestondepositionSavingaccount80TTB_CurrentAmt { get; set; }
        public double ChapterVIA_InterestondepositionSavingaccount80TTB_LimitAmt { get; set; }
        public double ChapterVIA_InterestondepositionSavingaccount80TTB_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_InterestOnLoanForPurchaseOfElectricVehicle80EEB_CurrentAmt { get; set; }
        public double ChapterVIA_InterestOnLoanForPurchaseOfElectricVehicle80EEB_LimitAmt { get; set; }
        public double ChapterVIA_InterestOnLoanForPurchaseOfElectricVehicle80EEB_EligibleDeductionAmt { get; set; }
        public DateTime ChapterVIA_VehiclePurchaseDate { get; set; }
        public double ChapterVIA_Donations80G_100Per_CurrentAmt { get; set; }
        public string ChapterVIA_Donations80G_100Per_Limit { get; set; }
        public double ChapterVIA_Donations80G_100Per_EligibleDeductionAmt { get; set; }
        public string ChapterVIA_Donations80G_100Per_Remark { get; set; }
        public double ChapterVIA_Donations80G_50Per_CurrentAmt { get; set; }
        public string ChapterVIA_Donations80G_50Per_Limit { get; set; }
        public double ChapterVIA_Donations80G_50Per_EligibleDeductionAmt { get; set; }
        public string ChapterVIA_Donations80G_50Per_Remark { get; set; }
        public double ChapterVIA_Donations80G_TempleMosqueGurudwara50Per_CurrentAmt { get; set; }
        public string ChapterVIA_Donations80G_TempleMosqueGurudwara50Per_Limit { get; set; }
        public double ChapterVIA_Donations80G_TempleMosqueGurudwara50Per_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_Donations80GGA_ScientificResearch_Ruraldevelopment_CurrentAmt { get; set; }
        public string ChapterVIA_Donations80GGA_ScientificResearch_Ruraldevelopment_Limit { get; set; }
        public double ChapterVIA_Donations80GGA_ScientificResearch_Ruraldevelopment_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_Donations80GGC_PoliticalParties_CurrentAmt { get; set; }
        public string ChapterVIA_Donations80GGC_PoliticalParties_Limit { get; set; }
        public double ChapterVIA_Donations80GGC_PoliticalParties_EligibleDeductionAmt { get; set; }
        public double ChapterVIA_TotalAmount { get; set; }

        //   public double TotalIncomeFromLetOutHouseProperty { get; set; }
        public bool IncomeFromLetOutHouseProperty_Submit { get; set; }
        public DateTime IncomeFromLetOutHouseProperty_SubmitDate { get; set; }
        public int IncomeFromLetOutHouseProperty_SubmitCount { get; set; }

        public double ChapterVIA_80DD_Handicap_Severeamount { get; set; }
        public double ChapterVIA_80DD_Handicap_Nonsevereamount { get; set; }

        public double ChapterVIA_80DDB_Seniorcitizen_Amount { get; set; }
        public double ChapterVIA_80DDB_Veryseniorcitizen_Amount { get; set; }

        public double ChapterVIA_80U_Physical_Handicap_Severe_Amount { get; set; }
        public double ChapterVIA_80U_Physical_Handicap_Nonsevere_Amount { get; set; }

        public double ChapterVIA_80CCD_1B_NPS_Amount { get; set; }

        public double ChapterVIA_80TTA_Amount { get; set; }

        public double ChapterVIA_80TTB_Amount { get; set; }

        public double ChapterVIA_80EEB_Interes_Onelecticvechile { get; set; }



        public float OtherIncome1 { get; set; }
        public string OtherIncome1_Remark { get; set; }
        public float OtherIncome2 { get; set; }
        public string OtherIncome2_Remark { get; set; }
        public float OtherIncome3 { get; set; }
        public string OtherIncome3_Remark { get; set; }
        public float TotalOtherIncome { get; set; }





        public int PreviousEmployerId { get; set; }
        public string PreviousEmployerId_Encrypted { get; set; }
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
    }
}