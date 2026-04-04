using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Leave
{
    public class Atten_PublicHoliday
    {
        [Key]
        public double PublicHolidayID { get; set; }
        public string PublicHolidayID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PublicHolidayBranchId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime PublicHolidayDate { get; set; }
        public int OTCoff { get; set; }
     
    }
}