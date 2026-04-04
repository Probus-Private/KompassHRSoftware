using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_LateMarkSetting
    {
        public double LateMarkSettingId { get; set; }
        public string LateMarkSettingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double LateMarkSettingBranchId { get; set; }
        public string LateMarkSettingName { get; set; }

        public double LateMarkSettingDetailsId { get; set; }
        public double FromLateMarkCount { get; set; }
        public double ToLateMarkCount { get; set; }
        public double Day { get; set; }

        public class LateMarkSettingDetails
        {
            public double FromLateMarkCount { get; set; }
            public double ToLateMarkCount { get; set; }
            public double Day { get; set; }
        }
    }
}