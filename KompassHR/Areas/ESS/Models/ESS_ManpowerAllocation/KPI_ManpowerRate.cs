using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class KPI_ManpowerRate
    {
        public double ManpowerRateId { get; set; }
        public string ManpowerRateId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { set; get; }
        public string MachineName { set; get; }
        public string ModifiedDate { set; get; }
        public double CmpID { get; set; }
        public double ManpowerRateBranchId { get; set; }
        public double ManpowerRateKPICategoryId { get; set; }
        public double ManpowerRateKPISubCategoryId  { get; set; }
        public string ManpowerRate { set; get; }
        public DateTime ManpowerRateMonth { get; set; }
        public DateTime ManpowerRateFromDate { get; set; }
        public DateTime ManpowerRateToDate { get; set; }
        public int ManpowerRateHeadCount { get; set; }
        public float ManpowerRateManDays { get; set; }
        //public DateTime ManpowerRateMonth { get; set; }
    }
}