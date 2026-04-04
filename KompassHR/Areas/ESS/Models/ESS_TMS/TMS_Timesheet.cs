using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TMS
{
    public class TMS_Timesheet
    {
        public double TimeSheetID { get; set; }
        public string TimeSheetID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double TimesheetEmployeeID { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public double ProjectID { get; set; }
        public double ModuleId { get; set; }
        public double ClientID { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public DateTime FormTime { get; set; }
        public DateTime ToTime { get; set; }
        public string TaskStatus { get; set; }
        public string CompletePercentage { get; set; }
        public double TaskCategoryId { get; set; }
        public double TaskSubCategoryId { get; set; }
        public string Time { get; set; }
        public string ResponsiblePerson { get; set; }

    }
}