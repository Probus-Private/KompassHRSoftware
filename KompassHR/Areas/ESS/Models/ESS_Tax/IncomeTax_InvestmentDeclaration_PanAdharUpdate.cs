using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_InvestmentDeclaration_PanAdharUpdate
    {
        public double EmployeeId { get; set; }
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyName { get; set; }
        public string BranchName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public string GradeName { get; set; }
        public string PAN { get; set; }
        public string AadhaarNo { get; set; }
        public bool PANAadhaarLink { get; set; }

    }
}