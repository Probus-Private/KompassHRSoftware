using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Canteen
{
    public class Canteen_GuestMaster
    {
        public double CanteenGuestID { get; set; }
        public string CanteenGuestID_Encrypted { get; set; }
        public double CanteenGuestHostEmployeeID { get; set; }      
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string NoOfGuest { get; set; }
        public string FoodPreference { get; set; }
        public bool MorningBreakfast { get; set; }
        public bool EveningBreakfast { get; set; }
        public bool Lunch { get; set; }
        public bool Dinner { get; set; }
        public bool Brunch { get; set; }
        public DateTime CanteenGuestFormDate { get; set; }
        public DateTime CanteenGuestToDate { get; set; }
        public string CanteenGuestRemark { get; set; }
    }
}