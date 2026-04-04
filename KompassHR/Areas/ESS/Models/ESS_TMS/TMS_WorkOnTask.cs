using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TMS
{
    public class TMS_WorkOnTask
    {
        public double WorkOnTaskID { get; set; }
        public string WorkOnTaskID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public double WorkOnTaskTaskAssignID { get; set; }
        public DateTime TaskDate { get; set; }
        public string WorkOnTask { get; set; }
        public string TaskStatus { get; set; }
        public string CompletePercentage { get; set; }
        public string FilePath { get; set; }
        public double KRA { get; set; }

        public Nullable<System.TimeSpan> WorkFromTime { get; set; }
        public Nullable<System.TimeSpan> WorkToTime { get; set; }

        public string TaskName { get; set; }
        public string TaskTime { get; set; }
        public string DocNos { get; set; }
    }
}