using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_BlockEmployee
    {
        public double BlockEmployeeId { get; set; }
        public string BlockEmployeeId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string BlockEmployeeName { get; set; }
        public string AadharNo { get; set; }
        public string PanNo { get; set; }
        public string Reason { get; set; }
        public bool IsBlock { get; set; }
        public string BlockedBy { get; set; }
    }
}