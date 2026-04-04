using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_LogDownloadSetting
    {
        public double AttenLogSettingId { get; set; }
        public string AttenLogSettingId_Encrypted { get; set; }
        public string LogSettingName { get; set; }
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DatabaseName { get; set; }
        public string TableName { get; set; }

        public string UserId { get; set; }
        public string DeviceId { get; set; }
        public string DownloadDate { get; set; }
        public string LogDate { get; set; }
        public string Direction { get; set; }
        public string AttDirection { get; set; }
        public string C6 { get; set; }
        public bool IsMonthWise { get; set; }
    }


}