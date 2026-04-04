using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_InvestmentDeclaration_LetOutHouseProperty
    {
        public double IncomeFromLetOutHousePropertyId { get; set; }
        public string IncomeFromLetOutHousePropertyId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
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
    }
}