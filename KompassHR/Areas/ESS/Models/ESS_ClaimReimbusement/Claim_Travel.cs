using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ClaimReimbusement
{
    public class Claim_Travel
    {
        public int VoucherNo { get; set; }
        public string TravelClaimId_Encrypted { get; set; }
        public int TravelClaimId { get; set; }
        public DateTime VoucherDate { get; set; }
        public string TravelType { get; set; }
        public double ClaimTravelPurposeId { get; set; }
        public int TravelClaimEmployeeId { get; set; }
        public string PerKMRate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string Description { get; set; }
        public double TotalKM { get; set; }
        public double TotalKMAmount { get; set; }
        public double Transport { get; set; }
        public double Food { get; set; }
        public double Hotel { get; set; }
        public string ConveyanceRemark { get; set; }
        public double Conveyance { get; set; }
        public string OtherARemark { get; set; }
        public double OtherA { get; set; }
        public string OtherBRemark { get; set; }
        public double OtherB { get; set; }
        public string OtherCRemark { get; set; }
        public double OtherC { get; set; }
        public double TotalAmount { get; set; }
        public string FoodPath { get; set; }
        public string TransportPath { get; set; }

        public string TotalKMPath { get; set; }
        public string HotelPath { get; set; }
        public string ConveyancePath { get; set; }
        public string OtherAPath { get; set; }
        public string OtherBPath { get; set; }

        public double ApprovedAmount { get; set; }
    }
}