using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class KPI_SubCategory
    {
        public double KPISubCategoryId { get; set; }
        public string KPISubCategoryId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { set; get; }
        public string MachineName { set; get; }
        public string ModifiedDate { set; get; }
        public double CmpID { get; set; }
     
        public double KPISubCategoryBranchId { get; set; }
        public string KPISubCategoryFName { get; set; }
        public bool IsDefault { get; set; }
    }

    public class KPI_DepartmentId
    {
        public int KPIDepartmentId { get; set; }
    }

    public class KPI_LoadDesignation
    {
        public double? CmpID { get; set; }
        public double? KPISubCategoryBranchId { get; set; }
    }
}