using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_Challan_Deductee
    {
        public int ChallanDeducteeID { get; set; }
        public string ChallanDeducteeID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public int IncomeTax_Challan_ChallanId { get; set; }
        public int ChallanDeducteeBranchId { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeSerialNo { get; set; }
        public int SrNoInChallanAsPerRunningSerialNo { get; set; }
        public string EmployeeReferenceNumberProvidedByEmployer { get; set; }
        public string Mode { get; set; }
        public string PANOfTheEmployee { get; set; }
        public string NameOfTheEmployee { get; set; }
        public string SectionCodeAnnexure2 { get; set; }
        public DateTime DateOfPaymentCredit { get; set; }
        public DateTime DateOfDeduction { get; set; }
        public double AmountPaidCredited { get; set; }
        public double TDSAmount { get; set; }
        public double SurchargeAmount { get; set; }
        public double EducationCessAmount { get; set; }
        public double TotalTDSAmount { get; set; }
        public double TotalTaxDepositedAmount { get; set; }
        public DateTime DateOfDeposite { get; set; }
        public string ReasonForNonDeductionLowerDeductionIfAny { get; set; }
        public string NumberOfCertificate { get; set;}
        public string ErrorDescription { get; set; }
        public bool IsActive { get; set; }
    }
}