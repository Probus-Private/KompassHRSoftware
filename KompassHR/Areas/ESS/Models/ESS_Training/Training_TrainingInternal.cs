using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Training
{
    public class Training_TrainingInternal
    {
        public double TrainingInternalId { get; set; }
        public string TrainingInternalId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int TrainingCategoryId { get; set; }
        public string TrainingCategory { get; set; }
        public string NoOfTrainingConducted { get; set; }
        public string ContractDuration { get; set; }
        public string PhotoPath { get; set; }
        public string LinkedinProfile { get; set; }
        public DateTime FromMonth { get; set; }
        public DateTime ToMonth { get; set; }
    }
}