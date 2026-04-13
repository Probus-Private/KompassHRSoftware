using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_TaxDeclaration
{
    public class Module_TaxDeclaration
    {
        public List<Dictionary<string, object>> DeclarationList { get; set; }
        public List<string> ColumnNames { get; set; }
    }

    public class IncomeTax_InvestmentDeclaration
    {
        
        public string InvestmentDeclarationId { get; set; }
        public string EmployeeId { get; set; }
        public string InvestmentDeclarationFyearId { get; set; }
        public string CompanyName { get; set; }
        public string BusinessUnit { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeNo { get; set; }
        public string JoiningDate { get; set; }
        public string Gender { get; set; }
        public string PAN { get; set; }
        public string AadharNo { get; set; }
        public string PANandAadharLink { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Grade { get; set; }
        public string TotalHRA { get; set; }
        public string AprLandlordPAN { get; set; }
        public string TotalDeclarationUnderSection80C { get; set; }
        public string EligibleDeductionUnderSection80C { get; set; }

        public string TotalRent { get; set; }
        public string US80C { get; set; }
        public string ChapterVIA { get; set; }
        public string SelfOccupiedHouseLoan { get; set; }
        public string LetOutHouse { get; set; }
        public string OtherIncome { get; set; }
        public string PreviousEmployer { get; set; }
       
    }

    public class DeleteMonthlyTaxCalculation
    {
        public int TaxCal_Id { get; set; }
        public int TaxCal_EmployeeID { get; set; }
        public DateTime TaxCal_MonthYear { get; set; }
    }

}