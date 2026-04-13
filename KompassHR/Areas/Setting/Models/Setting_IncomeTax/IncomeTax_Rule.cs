using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_IncomeTax
{
    public class IncomeTax_Rule
    {
        public int IncomeTaxRuleId { get; set; }
        public string IncomeTaxRule_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int IncomeTaxRule_FyearId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public double HRA_Metro_Percentage { get; set; }
        public double HRA_Metro_Minus { get; set; }
        public double HRA_NoNMetro_Percentage { get; set; }
        public double HRA_NoNMetro_Minus { get; set; }
        public bool HRA_Calculation_Monthly_Yearly { get; set; }
        public double HRA_PanLimit { get; set; }
        public double ChildEducation_Allowance { get; set; }
        public string ChildEducation_NoOfChildren { get; set; }
        public double ChildHostel_Allowance { get; set; }
        public double ChildHostel_NoOfChildren { get; set; }
        public double Conveyance_Handicap_Amount { get; set; }
        public double LeaveSalary_Amount { get; set; }
        public double Gratuity_Coverd_Amount { get; set; }
        public double Gratuity_NotCoverd_Amount { get; set; }
        public double StandardEducation_OldRegime_Amount { get; set; }
        public double StandardEducation_NewRegime_Amount { get; set; }
        public double ProfessionalTax_Amount { get; set; }
        public double TaxRule80C_Amount { get; set; }
        public double TaxRule80TTA_Amount { get; set; }
        public double TaxRule80TTB_Amount { get; set; }
        public double TaxRule80D_ParentsBelow60_Self_Family_Children { get; set; }
        public double TaxRule80D_ParentsBelow60_Parents { get; set; }
        public double TaxRule80D_ParentsBelow60_Deduction80D { get; set; }
        public double TaxRule80D_Below60_ParentsAbove60_Self_Family_Children { get; set; }
        public double TaxRule80D_Below60_ParentsAbove60_Parents { get; set; }
        public double TaxRule80D_Below60_ParentsAbove60_Deduction80D { get; set; }
        public double TaxRule80D_BothAbove60_Self_Family_Children { get; set; }
        public double TaxRule80D_BothAbove60_Parents { get; set; }
        public double TaxRule80D_BothAbove60_DeductionUnder80D { get; set; }
        public double TaxRule80D_BothAbove60_DeductionUnder80D_HealthCheckup { get; set; }
        public double Housingloan_After_1999 { get; set; }
        public double Housingloan_Before_1999 { get; set; }
        public double Housingloan_RepairOrRenewal { get; set; }
        public double TaxRule80CCD_1B_NPS_Amount { get; set; }
        public double TaxRule80DD_Handicap_SevereAmount { get; set; }
        public double TaxRule80DD_Handicap_NonSevereAmount { get; set; }
        public double TaxRule80DDB_SeniorCitizen_Amount { get; set; }
        public double TaxRule80DDB_NormalCitizen_Amount { get; set; }
        public double TaxRule80U_Physical_Handicap_Severe_Amount { get; set; }
        public double TaxRule80U_Physical_Handicap_Nonsevere_Amount { get; set; }
        public double TaxRule80EEB_Interest_OnElecticVechile { get; set; }
        public double IncomefromLetOutHouseProperty_StandardEducation { get; set; }
        public double TaxRule80EE_Limit { get; set; }
        public double TaxRule80EE_Loan_Maxamount { get; set; }
        public double TaxRule80EE_PropertyValues { get; set; }
        public double TaxRule80EEA_Limit { get; set; }
        public double TaxRule80EEA_PropertyValues { get; set; }
        public double HPLoss_Maxamount { get; set; }
        public double TaxRule_Rabate_OldRegime { get; set; }
        public double TaxRule_Rabate_NewRegime { get; set; }
    }
}