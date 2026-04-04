using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance
{
    public class Mas_Payroll_LoanType
    {
        
            public long LoanTypeId { get; set; }  // numeric(18,0) Unchecked
            public string LoanTypeId_Encrypted { get; set; }
            public bool? Deactivate { get; set; }
            public bool? UseBy { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? CreatedDate { get; set; }
            public string ModifiedBy { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public string MachineName { get; set; }
            public string LoanType { get; set; }  // nvarchar(100) Unchecked
        
    }
}