using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Travel
{
    public class Travel_Plan_LocalTransport
    {
        public int LocalTranPlanID { get; set; }
        public string LocalTranPlanID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int TravelBranchID { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public int TravelCalenderID { get; set; }
        public int LTPlanEmployeeId { get; set; }
        public string PlanType { get; set; }
        public string Mode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Note { get; set; }
        public string BookingType { get; set; }
        public string BookingStatus { get; set; }
        public int FareCharges { get; set; }
        public string CancelPlanDescription { get; set; }
        public bool TravelConfirmed { get; set; }
        public bool Reimbursement { get; set; }
        public bool SettlementDone { get; set; }
        public string BKCabType { get; set; }
        public string BKCabContactPerson { get; set; }
        public string BKCabBookingInstruction { get; set; }
        public string RejectionRemark { get; set; }
        public string EmployeeName { get; set; }
        public string TravelCalenderID_Encrypted { get; set; }
        
    }
}