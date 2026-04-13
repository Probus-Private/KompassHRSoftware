using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class Mas_Payroll_LoanType
    {
        public long LoanTypeId { get; set; }                // numeric(18,0)
        public string LoanTypeId_Encrypted { get; set; }    // nvarchar(70)
        public bool? Deactivate { get; set; }               // bit (nullable)
        public bool? UseBy { get; set; }                    // bit (nullable)
        public string CreatedBy { get; set; }               // nvarchar(100)
        public DateTime? CreatedDate { get; set; }          // datetime
        public string ModifiedBy { get; set; }              // nvarchar(100)
        public DateTime? ModifiedDate { get; set; }         // datetime
        public string MachineName { get; set; }             // nvarchar(100)
        public string LoanType { get; set; }
    }
}