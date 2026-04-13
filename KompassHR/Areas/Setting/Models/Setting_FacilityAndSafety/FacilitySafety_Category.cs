using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_FacilityAndSafety
{
    public class FacilitySafety_Category
    {
        public double FacilitySafetyCategoryID { get; set; }
        public string FacilitySafetyCategoryID_Encrypted { get; set; }     
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool IsActive { get; set; }
        public string FacilitySafetyCategoryName { get; set; }
    }
}