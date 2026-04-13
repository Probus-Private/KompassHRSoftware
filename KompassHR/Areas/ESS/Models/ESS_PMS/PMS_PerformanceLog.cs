using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_PerformanceLog
    {
        public long ObjectivePerformanceId { get; set; }
        public string ObjectivePerformanceId_Encrypted { get; set; }
        public bool? Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public DateTime? Date { get; set; }
        public long ObjectiveId { get; set; }
        public int PercentageValue { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    }
}