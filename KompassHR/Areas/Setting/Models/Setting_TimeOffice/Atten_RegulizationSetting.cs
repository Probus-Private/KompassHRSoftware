using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_RegulizationSetting
    {
        public double RegulizationSettingId { get; set; }
        public string RegulizationSettingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public TimeSpan ? FromTime { get; set; }
        public TimeSpan ? ToTime { get; set; }
        public string Type { get; set; }
        
    }
}