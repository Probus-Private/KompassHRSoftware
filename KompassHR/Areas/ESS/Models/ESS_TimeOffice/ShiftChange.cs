using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class ShiftChange
    {
        public double ShiftChangeID { get; set; }
        public string ShiftChangeID_Encrypted { get; set; }
        public double ShiftChangeShiftId { get; set; }
        public int DocNo { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ShiftChangeReason { get; set; }
        public double ShiftChangeEmployeeid { get; set; }

    }
}