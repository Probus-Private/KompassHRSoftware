using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_EmployeeTransfer
{
    public class Trans_BusinessUnit
    {
        public int TransferBusinessUnitId { get; set; }
        public string TransferBusinessUnitId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public double TransferEmployeeBranchId { get; set; }
        public double TransferEmployeeId { get; set; }
        public double TransferToBranchId { get; set; }
        public string TransferReportingDate { get; set; }
        public double TransferReportingHRId { get; set; }
        public string TransferStatus { get; set; }
        public double TransferApprovalLevel { get; set; }
        public bool TransferSetUpAdmin { get; set; }
        public bool TransferSetUpAttendance { get; set; }
        public double TransferOrderBy { get; set; }
        public string TransferReason { get; set; }
        public double TransferToCmpID { get; set; }
    }
}