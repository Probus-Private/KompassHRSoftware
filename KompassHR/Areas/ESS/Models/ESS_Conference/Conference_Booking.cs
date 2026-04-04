using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Conference
{
    public class Conference_Booking
    {
        public double ConferenceBookingID { get; set; }
        public string ConferenceBookingID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public bool Deactivate { get; set; }
        //public string CreatedBy { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public string ModifiedBy { get; set; }
        //public string ModifiedDate { get; set; }
        //public string MachineName { get; set; }

        public DateTime BookingDate { get; set; }
        public double ConferenceID { get; set; }
        public string Subject { get; set; }
        public string ConferenceDescription { get; set; } // Not in Table
        public string Description { get; set; }
        public Nullable<System.TimeSpan> FromTime { get; set; }
        public Nullable<System.TimeSpan> ToTime { get; set; }
        public string TotalDuration { get; set; }
    }
}