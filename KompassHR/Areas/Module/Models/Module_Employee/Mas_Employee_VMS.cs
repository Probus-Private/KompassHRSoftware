using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_VMS
    {
        public double EmployeeVMSId { get; set; }
        public string EmployeeVMSId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double EmployeeVMSEmployeeId { get; set; }
        public string ExtensionNo { get; set; }
        public string FloorNo { get; set; }
        public string CabinNo { get; set; }
        public bool IsVMSMultiLocationApplicable { get; set; }
    }
}