using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class KPI_Category
    {
        public double KPICategoryId { get; set; }
        public string KPICategoryId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { set; get; }
        public string MachineName { set; get; }
        public string ModifiedDate { set; get; }
        public double CmpID { get; set; }
        public double KPICategoryBranchId { get; set; }
        public string KPICategoryName { get; set; }
       
    }

}