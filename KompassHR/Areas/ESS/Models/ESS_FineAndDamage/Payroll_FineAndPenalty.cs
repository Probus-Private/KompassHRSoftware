using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_FineAndDamage
{
    public class Payroll_FineAndPenalty
    {
        public double PenaltyID { get; set; }
        public string PenaltyID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string PenaltyName { get; set; }
        public double PenaltyEmployeeID { get; set; }
        public string PenaltyDescription { get; set; }
        public DateTime DeductionMonth { get; set; }
        public string PenaltyAmount { get; set; }
        public string PenaltyRemark { get; set; }
    }
}