using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_LogColumnSettings
    {
        public long USBId { get; set; }
        public string USBId_Encrypted { get; set; }
        public bool? Deactivate { get; set; }
        public bool? UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string USBName { get; set; }
        public int? UserId { get; set; }
        public int? LogDate { get; set; }
        public int? DeviceId { get; set; }
        public int? Direction { get; set; }
        public string DirectionIn { get; set; }
        public string DirectionOut { get; set; }
    }
}