using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Warning
{
    public class Warning_Employee
    {
        public double WarningEmpID { get; set; }
        public string WarningEmpID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double WarningEmployeeID { get; set; }
        public double WarningID { get; set; }
        public double EmployeerID { get; set; }
        //public double EmployeeName { get; set; }
        public int DocNo { get; set; }
        public DateTime?  DocDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string RejectRemarks { get; set; }
        public string EmployeeName { get; set; }
        public string Warning_Type { get; set; }
    }
}