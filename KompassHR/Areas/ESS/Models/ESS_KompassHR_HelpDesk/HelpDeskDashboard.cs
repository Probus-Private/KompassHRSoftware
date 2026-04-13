using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_KompassHR_HelpDesk
{
    public class HelpDeskDashboard
    {
        public int OpenCount { get; set; }
        public int InProcessCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
        public int TotalCount { get; set; }
    }
}