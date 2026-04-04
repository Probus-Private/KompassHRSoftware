using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_FacilityAndSafety
{
    public class FacilitySafety_Master
    {
        public double FacilityID { get; set; }
        public string FacilityID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public double FacilitySafetyCategoryID { get; set; }
        public string Description { get; set; }
        public DateTime DocDate { get; set; }
        public string FilePathPhoto1 { get; set; }
        public string FilePathPhoto2 { get; set; }
        public string FilePathPhoto3 { get; set; }
        public double FacilityEmployeeID { get; set; }
    }
}