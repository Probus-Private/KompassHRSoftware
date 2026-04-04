using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class UpdateLineManpowerDesignation
    {
        public int CmpId { get; set; }
        public int BranchId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
    }
}