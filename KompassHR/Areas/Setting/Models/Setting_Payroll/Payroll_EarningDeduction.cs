using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Payroll
{
    public class Payroll_EarningDeduction
    {
        public double EarningDeductionID { get; set; }
        public string EarningDeductionID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string EarningDeductionName { get; set; }
        public string EarningDeductionShortName { get; set; }
        public string EarningDeductionType { get; set; }
        public string CTCVariableType { get; set; }
        public string PayableFixedType { get; set; }
                                                    
    }
}