using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ManpowerAllocation
{
    public class Manpower_ContractorWiseRate
    {
        public double ContractorWiseRateMasterId { get; set; }
        public string ContractorWiseRateMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { set; get; }
        public string MachineName { set; get; }
        public string ModifiedDate { set; get; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double ContractorId { get; set; }
        public double PerDayRate { get; set; }
        public double CanteenRate { get; set; }
        public double AttendanceBonusRate { get; set; }
        public double TransportAllowanceRate { get; set; }
        public double Other1 { get; set; }
        public double Other2 { get; set; }
        public string ServiceCharges { set; get; }
        public string SupervisorCharges { set; get; }
        public double DesignationId { get; set; }
        public string Rate { get; set; }
        public string Percentage { get; set; }
        public DateTime FromMonth { get; set; }
        public DateTime ToMonth { get; set; }
        public bool IsGross { get; set; }
        public bool IsOT { get; set; }

        public double ContractorWiseRateDetailsId { get; set; }
        public string DesignationName { set; get; }
    }
}