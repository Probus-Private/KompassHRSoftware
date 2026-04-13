using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_ClaimAndReimbusement
{
    public class Claim_ExpenseCategory
    {
        public double ExpenseCategoryID { get; set; }
        public string ExpenseCategoryID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string ExpenseCategoryName { get; set; }
    }
}