using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class Manpower_LineManpowerCategoryMap
    {
        public double LineManpowerCategoryMapId { get; set; }
        public string LineManpowerCategoryMapId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool IsActive { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double LineId { get; set; }
        public double UnitId { get; set; }
        public double KPISubCategoryId { get; set; }
        public string KPISubCategoryFName { get; set; }
    }
}