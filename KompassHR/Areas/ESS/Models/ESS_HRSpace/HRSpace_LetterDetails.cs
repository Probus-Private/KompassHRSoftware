using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_HRSpace
{
    public class HRSpace_LetterDetails
    {
        public int Letter_DetailId { get; set; }
        public string Letter_DetailId_Encrypeted { get; set; }
        public Boolean Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int LetterId { get; set; }
        public int Letter_EmployeeId { get; set; }
        public string Description { get; set; }
        public string LetterName { get; set; }
        public DateTime ClosingDate { get; set; }
        public int IsConfirm { get; set; }
        public string LetterHeader { get; set; }
        public string LetterFooter { get; set; }
    }
}