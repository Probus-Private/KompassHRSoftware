using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_FineAndDamage
{
    public class Payroll_Damage
    {
        public double DamageID { get; set; }
        public string DamageID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string DamageName { get; set; }
        public string DamageDescription { get; set; }
        public DateTime DeductionInMonth { get; set; }
        public double DamageEmployeeID { get; set; }
        public string DamageAmount { get; set; }
        public double RequesterID { get; set; }
        public string RequesterName { get; set; }
    }
}