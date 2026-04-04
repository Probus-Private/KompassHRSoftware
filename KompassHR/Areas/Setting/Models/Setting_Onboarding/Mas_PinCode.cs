using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Onboarding
{
    public class Mas_PinCode
    {
        public double PinId { get; set; }
        public string PinId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string PinCode { get; set; }
        public string StateName { get; set; }
        public string DistrictName { get; set; }
        public string TalukaName { get; set; }
        public string OfficeName { get; set; }
        public string SearchType { get; set; }
    }
}