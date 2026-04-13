using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ClaimReimbusement
{
    public class Claim_ExpenseCategoryMaster
    {
        public double ExpenceCategory_ID { get; set; }
        public string ExpenceCategory_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string ExpenceCategoryName { get; set; }
    }
}