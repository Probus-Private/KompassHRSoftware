using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_PunchSetting
    {
        public double PunchID { get; set; }
        public string PunchID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double SiteCode { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double PS_Name { get; set; }
        public double PS_LimitForIn { get; set; }
        public double PS_LimitForOut { get; set; }
        public double PS_BackDateDays { get; set; }
       
    }
}