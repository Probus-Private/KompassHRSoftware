using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_LogUpload
    {
        public double Log_SettingId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class AttenLogSettingViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsMonthWise { get; set; }
    }

    public class DeviceLogModel
    {
        public long DeviceId { get; set; }
        public string UserId { get; set; }
        public DateTime LogDate { get; set; }
        public string Direction { get; set; }
    }

    public class LogColumnSettings
    {
        public int Fid { get; set; }
        public string FieldName { get; set; }
        public  int ColumnIndex { get; set; }
    }

}