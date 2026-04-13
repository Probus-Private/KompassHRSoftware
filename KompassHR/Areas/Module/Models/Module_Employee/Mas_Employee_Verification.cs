using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Verification
    {

        public double VerificationID { get; set; }
        public string VerificationID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double VerificationEmployeeID { get; set; }
        public double VerificationType { get; set; }
        public DateTime VerificationEntrydate { get; set; }
        public string VerificationRemark { get; set; }
        public string VerificationName { get; set; }


    }
}