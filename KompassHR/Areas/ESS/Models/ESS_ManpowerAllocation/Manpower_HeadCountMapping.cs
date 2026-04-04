using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class Manpower_HeadCountMapping
    {
        public double HeadCountMappingMasterID { get; set; }
        public double HeadCountMappingDetailsID { get; set; }

        public string HeadCountMappingMasterID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public DateTime MonthYear { get; set; }
        public double LineId { get; set; }
        public string LineName { get; set; }
        public double KPISubCategoryId { get; set; }
        public float LineR1SCDay { get; set; }
        public float LineR2SCDay { get; set; }
        public float LineR3SCDay { get; set; }
        public float R1HeadCount { get; set; }
        public float R1MonthDays { get; set; }
        public float R2HeadCount { get; set; }
        public float R2MonthDays { get; set; }
        public float R3HeadCount { get; set; }
        public float R3MonthDays { get; set; }

    }


    public class Manpower_HeadCountMapping_Bulk_excel
    {
        public string LineName { get; set; }
        public double LineId { get; set; }
        public double KPISubCategoryId { get; set; }
        public string KPISubCategoryFName { get; set; }
        public double R1HeadCount { get; set; }
        public double R2HeadCount { get; set; }
        public double R3HeadCount { get; set; }

    }
}