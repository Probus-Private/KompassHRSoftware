using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_VMS
{
    public class Visitor_Mas_ARD
    {
        public double ARDId { get; set; }
        public string ARDId_Encrypted { get; set; }
        public string  CreatedBy { get; set; }
        public string MachineName { get; set; }
        public string ARDItemName { get; set; }
        public int ARDItemType { get; set; }
    }
}