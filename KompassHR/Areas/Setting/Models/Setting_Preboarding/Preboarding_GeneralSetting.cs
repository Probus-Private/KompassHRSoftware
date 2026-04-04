using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Preboarding
{
    public class Preboarding_GeneralSetting
    {
        public double PreGeneralSettingId { get; set; }
        public string PreGeneralSettingId_Encrypted { get; set; }
        public string AgeLimitYear { get; set; }
        public int PreboardingLinkExpiryDays { get; set; }
        public bool IncludingWorkingDay { get; set; }
        public string PreboardingLink { get; set; }
        public bool EmailBCC_IsRequired { get; set; }
        public string Email_SubjectForPreboarding { get; set; }
        public string Email_BodyPreboarding { get; set; }
        public bool IsSendSMS { get; set; }
        public string SMSTemplate { get; set; }
        public bool IsSendWhatsapp { get; set; }
        public int ReporingToHrAlertDays_Reporting { get; set; }
        public int ReportingToHrAlertDays_CandidateNotLogin { get; set; }

    }
}