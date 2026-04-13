using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Reporting
    {
        public double ReportingID { get; set; }
        public string ReportingID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ReportingEmployeeID { get; set; }
        public double ReportingManager1 { get; set; }
        public double ReportingManager2 { get; set; }
        public double ReportingHR { get; set; }
        public double ReportingAccount { get; set; }
        public double ReportingModuleID { get; set; }
        public bool IsCompulsory { get; set; }
        public string ApproverLevel { get; set; }
    }
    public class ModuleIdList
    {
        public double ReportingModuleID { get; set; }
    }

    public class RequestApproval
    {
        public double ModuleId { get; set; }
        public double Level1ReportingManagerId { get; set; }
        public double Level2ReportingManagerId { get; set; }
        public double Level3ReportingManagerId { get; set; }
        //public bool IsCompulsory { get; set; }
        //public double ApproverLevel { get; set; }
    }
}