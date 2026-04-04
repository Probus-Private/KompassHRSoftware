using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Preboarding
{
    public class Preboarding_EmailTemplate
    {
        public double EmailTemplateID { get; set; }
        public string EmailTemplateID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double DocOrigin { get; set; }
        public bool SendEmail { get; set; }
        public bool BCCIsRequired { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public bool SendSMS { get; set; }
        public string SMSBody { get; set; }
        public bool SendWhatsapp { get; set; }
        public string WhatsappBody { get; set; }
    }
}