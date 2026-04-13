using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class Manpower_IDELock
    {
        public int IDEId { get; set; }
        public string IDE_Encrypted_Id { get; set; }
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public DateTime MonthYear { get; set; }
        public Boolean Islock { get; set; }
    }
}