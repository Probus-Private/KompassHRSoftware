using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Inbox
{
    public class Inbox
    {
        public List<RecordList> RecordList { get; set; }
    }
    public class RecordList
    {
        public string DocID { get; set; }
        public string Origin { get; set; }
        public string Status { get; set; }
        public string ApproveRejectRemark { get; set; }
    }
}