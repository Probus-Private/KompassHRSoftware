using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_EventAndNews
{
    public class Event_Reward
    {
        public double RewardID { get; set; }
        public string RewardID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public double RewardEmployeeID { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string RewardTitle { get; set; }
        public string RewardDescripition { get; set; }
        public DateTime RewardFromDate { get; set; }
        public DateTime RewardToDate { get; set; }
        public string FilePath { get; set; }
        public bool IsActive { get; set; }
    }
}