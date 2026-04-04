using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class InvestmentChapterVI_A
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
        public DateTime? ChapterVIA_VehiclePurchaseDate { get; set; }
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

        public double TotalIncomeFromLetOutHouseProperty { get; set; }
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


        public double ChapterVIA_Medical80D_Self_HealthCheckup_CurrentAmt { get; set; }
        public double ChapterVIA_Medical80D_Self_HealthCheckup_LimitAmt { get; set; }
        public double ChapterVIA_Medical80D_Self_HealthCheckup_EligibleDeductionAmt { get; set; }


        public double ChapterVIA_Medical80D_Self_EligibleDeductionAmtlblShow { get; set; }
        public double ChapterVIA_Medical80D_Parents_EligibleDeductionAmtlblShow { get; set; }

    }

    public class Add80G_A
    {
        public string ChapterVIA_Donations80G_100Per_Remark { get; set; }
    }
    public class Add80G_B
    {
        public string ChapterVIA_Donations80G_50Per_Remark { get; set; }
    }
}
