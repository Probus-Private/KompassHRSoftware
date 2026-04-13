using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
   
    public class CheckInOut
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double DatabaseLatitude { get; set; }
        public double DatabaseLongitude { get; set; }
        public double CheckInOutLocationId { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public double CheckInOutId { get; set; }
        public string Remark { get; set; }

        public double EmployeeId { get; set; }
    }

    public class BulkCheckInOut
    {
        public string EmployeeNo { get; set; }
        public string EmployeeCardNo { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
    }

    public class OTApprovalList
    {
        public int ActualOTHrs { get; set; }
        public int EmployeeID { get; set; }
        public int InOutID { get; set; }
        public string Remark { get; set; }

    }

    public class COffApprovalList
    {
        public double ApprovedCoffDays { get; set; }
        public int EmployeeID { get; set; }
        public int InOutID { get; set; }
        public string Remark { get; set; }
    }

    public class ApproveRejectCheckInOut
    {
        public List<CheckInOut> ObjCheckInOut { get; set; }

    }

    public class CheckInOutApproval
    {
        public double EmployeeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
       
    }
}