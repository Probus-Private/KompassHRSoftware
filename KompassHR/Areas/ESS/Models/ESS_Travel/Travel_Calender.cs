using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Travel
{
    public class Travel_Calender
    {
        public int TravelCalenderID { get; set; }
        public string TravelCalenderID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; } 
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double TravelBranchID { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public double TravelCalenderEmployeeId { get; set; }
        public double TravelCalenderTravelPurposeID { get; set; }
        public string TravelType { get; set; }
        public DateTime TourStart { get; set; }
        public DateTime TourEnd { get; set; }
        public float NoofDays { get; set; }
        public string TravelNote { get; set; }
        public string PurposeDescription { get; set; }
        public bool Confidential { get; set; }
        public string TravelStatus { get; set; }
        public int AdditionalEmployee { get; set; }
        public double ApproveRejectBy { get; set; }
        public string Remark { get; set; }
        public string DayType { get; set; }
        public string TravelPurpose { get; set; }
        
    }
}