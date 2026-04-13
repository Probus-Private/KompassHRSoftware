using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Training
{
    public class Training_TrainingConduct
    {
        public double TrainingPlan_MasterId { get; set; }
        public string TrainingPlan_MasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string MachineName { get; set; }
        public double TrainingCalenderId { get; set; }
        public string TrainingCalenderName { get; set; }
        public string Batch { get; set; }
        public double EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public bool IsActive { get; set; }

    }
}