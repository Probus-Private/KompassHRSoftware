using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_PMS
{
    public class DateSetting
    {
        public double DateSettingId { get; set; }
        public string DateSettingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string QuarterID { get; set; }
        public string FromMonth { get; set; }
        public string ToMonth { get; set; }
    }
}