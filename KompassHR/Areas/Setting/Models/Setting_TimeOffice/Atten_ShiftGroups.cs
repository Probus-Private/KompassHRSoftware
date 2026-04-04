using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_ShiftGroups
    {
        public double? ShiftGroupId { get; set; }
        public string ShiftGroupId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double? CmpID { get; set; }
        public double? ShiftGroupBranchId { get; set; }
        public string ShiftGroupFName { get; set; }
        public string ShiftGroupSName { get; set; }
        public bool SecondDayShiftApplicable { get; set; }
        public bool IsDefault { get; set; }

    }
}