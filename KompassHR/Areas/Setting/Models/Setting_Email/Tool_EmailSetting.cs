using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Email
{
    public class Tool_EmailSetting
    {
        public int EmailID { get; set; }
        public string EmailID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public int Origin { get; set; }
        public string SMTPServerName { get; set; }
        public string PortNo { get; set; }
        public bool SSL { get; set; }
        public string FromEmailId { get; set; }
        public string Password { get; set; }
    }
}