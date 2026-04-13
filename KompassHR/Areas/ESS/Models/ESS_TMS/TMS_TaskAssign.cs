using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TMS
{
    public class TMS_TaskAssign
    {
        public double TaskAssignID { get; set; }
        public string TaskAssignID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public bool Deactivate { get; set; }    
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
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public double AssignToEmployeeID { get; set; }
        public string Priority { get; set; }
        public string Weightage { get; set; }
        public double AssignerEmployeeID { get; set; }
        public string TaskTittle { get; set; }
        public Nullable<System.TimeSpan> FromTime { get; set; }
        public Nullable<System.TimeSpan> ToTime { get; set; }
        public string TotalDuration { get; set; }
        public double TaskCategoryId { get; set; }
        public double TaskSubCategoryId { get; set; }
        public string TaskType { get; set; }

        public string Time { get; set; }
    }
}