using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_UploadDocument
    {
        public double EmployeeUploadDocumentID { get; set; }
        public string EmployeeUploadDocumentID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double DocumentID { get; set; }
        public double DocumentEmployeeId { get; set; }
        public string DocumentPath { get; set; }
        public string UploadDocumentName { get; set; }
    }
}