using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_VMS
{
    public class Visitor_Mas_Badges
    {
        public double BadgesId { get; set; }
        public string BadgesId_Encrypted { get; set; }
        public string MachineName { get; set; }
        public string CreatedBy { get; set; }
        public string Badges { get; set; }
    }
}