using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_LoanAndAdvance
{
    public class Payroll_Loan
    {
        public long LoanID { get; set; }  
        public string LoanID_Encrypted { get; set; }
        public long? CmpID { get; set; }
        public long? BranchID { get; set; }
        public bool? Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime? DocDate { get; set; }
        public string LoanTypeId { get; set; }
        public long? LoanEmployeeID { get; set; }
        public long? LoanAmount { get; set; }
        public long? LoanInterest { get; set; }
        public long? InterestAmount { get; set; }
        public long? TotalLoanAmount { get; set; }
        public long? NoOfInstallment { get; set; }
        public long? MonthlyDeductionAmount { get; set; }
        public string Reason { get; set; }
        public DateTime? DeducationStartDate { get; set; }
        public long? GuarantorName1 { get; set; }
        public long? GuarantorName2 { get; set; }
        public string Status { get; set; }
        public long? ManagerID1 { get; set; }
        public long? ManagerID2 { get; set; }
        public long? HRID { get; set; }
        public long? ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public DateTime? ApproveRejectDate { get; set; }
        public bool? ApprovedAutoManually { get; set; }
        public bool? CancelFlag { get; set; }
        public long? CancelBy { get; set; }
        public string CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string RequestFrom { get; set; }
        public string AdditionNotify { get; set; }
        public DateTime SettledDate { get; set; }
        public string SettledRemark { get; set; }
    }
}