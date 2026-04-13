using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Bank
    {
        public double EmployeeBankId { get; set; }
        public string EmployeeBankId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double EmployeeBankEmployeeId { get; set; }
        public string SalaryIFSC { get; set; }
        public string SalaryBankName { get; set; }
        public string SalaryAccountNo { get; set; }
        public string SalaryBankAddress { get; set; }
        public string SalaryBankBranchName { get; set; }
        public string SalaryNameAsPerBank { get; set; }
        public string PermanentIFSC { get; set; }
        public string PermanentBankName { get; set; }
        public string PermanentAccountNo { get; set; }
        public string PermanentBankAddress { get; set; }
        public string PermanentBankBranchName { get; set; }
        public string PermanentNameAsPerBank { get; set; }
        public string SalaryConfirmAccountNo { get; set; }
        public string PermanentConfirmAccountNo { get; set; }
        public string SalaryMode { get; set; }
    }
}