using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_VMS
{
    public class Visitor_Mas_VisitorType
    {
        public double VisitorTypeId { get; set; }
        public string VisitorTypeId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string VisitorType { get; set; }
    }
}