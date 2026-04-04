using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Logistic
{
    public class Logistics_Booking
    {
        public double CourierBookingId { get; set; }
        public string CourierBookingId_Encrypted{ get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double DocNo { get; set; }
        public string DocDate { get; set; }
        public string BookingEmployeeId { get; set; }
        public string ParcelType { get; set; }
        public string Description { get; set; }
        public string CourierServiceProviderId { get; set; }
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public double TrackingNumber { get; set; }
        public double CourierCharges { get; set; }
        public string HandOverCourier { get; set; }
        public string Attachment { get; set; }
        public bool IsConfidential { get; set; }
    }
}