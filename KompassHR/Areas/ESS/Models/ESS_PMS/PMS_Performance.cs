using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_Performance
    {
        public long PerformanceId { get; set; } 
        public string PerformanceId_Encrypted { get; set; } 
        public bool? Deactivate { get; set; } 
        public bool? UseBy { get; set; } 
        public string CreatedBy { get; set; } 
        public DateTime? CreatedDate { get; set; } 
        public string ModifiedBy { get; set; } 
        public DateTime? ModifiedDate { get; set; } 
        public string MachineName { get; set; } 
        public long? CmpId { get; set; } 
        public DateTime? KraUpdatedDate { get; set; } 
        public long? OwnObjectiveCreationId { get; set; } 
        public float PercentageValue { get; set; } 
        public string Status { get; set; } 
        public double ActualValue { get; set; }
        public string Comment { get; set; } 
        public Double ObjectivePeriodId { get; set; }
        public string Attachment { get; set; }
    }

}