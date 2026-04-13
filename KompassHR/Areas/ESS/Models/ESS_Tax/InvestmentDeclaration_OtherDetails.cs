using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class InvestmentDeclaration_OtherDetails
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
        public float OtherIncome1 { get; set; }
        public string OtherIncome1_Remark { get; set; }
        public float OtherIncome2 { get; set; }
        public string OtherIncome2_Remark { get; set; }
        public float OtherIncome3 { get; set; }
        public string OtherIncome3_Remark { get; set; }
        public float TotalOtherIncome { get; set; }

    }
}