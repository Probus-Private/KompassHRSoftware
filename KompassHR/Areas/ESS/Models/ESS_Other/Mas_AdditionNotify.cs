using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Other
{
    public class Mas_AdditionNotify
    {
        public int AdditionNotifyId { get; set; }
        public string AdditionNotifyId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double AdditionNotifyEmployeeID { get; set; }
        public double AdditionNotifyIdTeamEmployeeID { get; set; }
        public string AdditionNotifyEmailID { get; set; }
    }
}