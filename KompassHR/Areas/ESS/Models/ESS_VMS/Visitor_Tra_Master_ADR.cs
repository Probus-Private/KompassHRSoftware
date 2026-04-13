using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_VMS
{
    public class Visitor_Tra_Master_ADR
    {
        public double AdrId { get; set; }
        public double VisitorId { get; set; }
        public string AdrVisitorName { get; set; }      
        public string AdrName { get; set; }
        public string Remark { get; set; }
        public string LockerNo { get; set; }
        public string Origin { get; set; }
        public double ItemID { get; set; }
        public double LockerID { get; set; }

    }
}