using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class Atten_WorkFromHome
    {
        public double WFHId { get; set; }
        public string WFHId_Encrypted { get; set; }
        public double CmpId { get; set; }
        public double WFHIdEmployeeId { get; set; }
        public double WFHIdBranchId { get; set; }
        public int DocNo { get; set; }
        public string WFHType { get; set; }
        public double WFHShiftId { get; set; }
        public string WFHShiftName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public TimeSpan FromTime { get; set; }
        public TimeSpan ToTime { get; set; }
        public int TotalMinutes { get; set; }
        public string TotalDuration { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string RequestFrom { get; set; }
        public string ApproveRejectRemark { get; set; }
        public string AdditionNotify { get; set; }
        public string ProofUpload { get; set; }

    }
}