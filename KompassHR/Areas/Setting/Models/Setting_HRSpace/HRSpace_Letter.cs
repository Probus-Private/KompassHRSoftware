using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_HRSpace
{
    public class HRSpace_Letter
    {
        public int LetterId { get; set; }
        public string LetterId_Encrypted { get; set; }
        public Boolean Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public string LetterName { get; set; }
        public string Letter_Description { get; set; }
        public string Letter_EmployeeId { get; set; }
        public int BranchId { get; set; }
        public Boolean IsSendToEmployee { get; set; }
        public int HRSpace_letterMaster_Id { get; set; }
        public string Letter_Header { get; set; }
        public string Letter_Footer { get; set; }
    }
}