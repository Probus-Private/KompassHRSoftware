using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_IncomeTax
{
    public class IncomeTax_TANBuMapping
    {
        public int TANBuMappingID { get; set; }
        public string TANBuMappingID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int TANBuMappingCmpId { get; set; }
        public int TANBuMappingBranchId { get; set; }
        public int TANId { get; set; }

    }
}