using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class Mapping_R1R2R3
    {
        public double R1R2R3MappingId { get; set; }
        public string R1R2R3MappingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public DateTime? R1FromDate { get; set; }
        public DateTime? R1ToDate { get; set; }
        public DateTime? R2FromDate { get; set; }
        public DateTime ? R2ToDate { get; set; }
        public DateTime ?  R3FromDate { get; set; }
        public DateTime ? R3ToDate { get; set; }
        public DateTime ? MonthYear { get; set; }
    }
}