using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_Challan
    {
       public int ChallanId { get; set; }
       public string ChallanId_Encrypted { get; set; }
       public bool Deactivate { get; set; }
       public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public DateTime DocDate { get; set; }
        public int ChallanNo { get; set; }
        public string TAN { get; set; }
        public int ChallanTANId { get; set; }
        public int ChallanFyearId { get; set; }
        public string Quarter { get; set; }
        public DateTime QuarterMonth { get; set; }
        public string ChallanBranchId { get; set; }
        public string ChallanEmployeeId { get; set; }
        public double TDSAmount { get; set; }
        public double SurchargeAmount { get; set; }
        public double EducationAmount { get; set; }
        public double InterstAmount { get; set; }
        public double FeeAmount { get; set; }
        public double PenaltyOtherAmount { get; set; }
        public double TotalTaxDepositedAmount { get; set; }
        public string BSR { get; set; }
        public string ChallanSerialNo { get; set; }
        public DateTime TaxDepositeDate { get; set; }
        public double MinorHeadOfChallan { get; set; }
        public double InterestToBeAllocatedApportioned { get; set; }
        public double OtherAmount { get; set; }
        public string NILLChallanIndicator { get; set; }
        public string Remark { get; set; }
        public int EmployeeCount { get; set; }
    }
}