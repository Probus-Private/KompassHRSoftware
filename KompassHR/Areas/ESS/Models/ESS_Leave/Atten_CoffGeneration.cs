using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Leave
{
    public class Atten_CoffGeneration
    {
        public string CoffGenerationID { get; set; }
        public string CoffGenerationID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double CoffGenerationBranchID { get; set; }
        public string Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public double CoffGenerationEmployeeID { get; set; }
        public string CoffGenerationManually { get; set; }
        public DateTime CoffExpriredDate { get; set; }
        public DateTime CoffGenerationDate { get; set; }
        public float Actualnoofcoff { get; set; }
        public float Approvenoofcoff { get; set; }
        public string Reason { get; set; }
        public string CoffApprovedStatus { get; set; }
        public string ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public DateTime ApproveRejectDate { get; set; }
        public DateTime ExtensionDate { get; set; }
        public string ExtensionRemark { get; set; }
        public double CoffAttenInOutId { get; set; }
        public float AvailedCoff { get; set; }
        public string RequestFrom { get; set; }
        public string CoffStatus { get; set; }
        public string CoffRequestID { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}