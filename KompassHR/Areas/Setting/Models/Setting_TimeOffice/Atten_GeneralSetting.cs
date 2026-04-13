using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_GeneralSetting
    {
        public double AttenGeneralId { get; set; }
        public string AttenGeneralId_Encrypted { get; set; }
        public double CmpId { get; set; }
        public string OutDoorCompanySettingName { get; set; }
        public int OutDoorCompanyShortPeriod { get; set; }
        public int OutDoorCompanyIstHalfDuration { get; set; }
        public int OutDoorCompanyIIndHalfDuration { get; set; }
        public int OutDoorCompanyFullDayDuration { get; set; }
        public bool AllowTeamRequest { get; set; }
        public bool IsBackDate { get; set; }
        public int BackDatedDays { get; set; }
        public double OutDoorCompanyBranchId { get; set; }
        public bool IsDefault { get; set; }
        public float? AutoApprovalDays { get; set; }
        public float FutureDatedDays { get; set; }

    }
}