using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_LWFCode
    {
        [Key]
        public double LWFCodeId { get; set; }
        public string LWFCodeId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
       // public double LWFCodeBranchId { get; set; }
        public string LWFCode { get; set; }
        public double LWFStateCode { get; set; }
        public string ResponsiblePerson { get; set; }
        public string CompanyName { get; set; }
        public string StateName { get; set; }
        public string LWFRemark { get; set; }
    }
}