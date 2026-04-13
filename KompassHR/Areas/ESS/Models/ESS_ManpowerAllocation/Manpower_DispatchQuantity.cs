using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class Manpower_HC_MD_DispatchQuantityMaster
    {
        public double DispatchQuantityMasterID { get; set; }
        public string DispatchQuantityMasterID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public DateTime Date { get; set; }
        public double LineId { get; set; }
        public string LineName { get; set; }
        public double KPISubCategoryId { get; set; }
        public string KPISubCategoryFName { get; set; }
        public float HeadCount { get; set; }
        public float ManDays { get; set; }
        public double DispatchQuantityDetailsID { get; set; }
    }
}