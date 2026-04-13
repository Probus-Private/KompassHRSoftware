using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_CompanyObjectiveCreation
    {
        public long CompanyObjectiveCreationId { get; set; }

        public string CompanyObjectiveCreationId_Encrypted { get; set; }

        public bool? Deactivate { get; set; }

        public bool? UseBy { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string MachineName { get; set; }

        public long? CmpId { get; set; }

        public string ObjectiveTitle { get; set; }

        public int? YearId { get; set; }

        public string GoalType { get; set; }

        public string GoalTitle { get; set; }

        public string SmartGoal { get; set; }

        public string GoalCategory { get; set; }

        public int? DepartmentId { get; set; }

        public string GoalOwner { get; set; }

        public string TargetType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public float ActualValue { get; set; }

        public float ProgressPercenatge { get; set; }

        public float Target { get; set; }

        public string UOM { get; set; }


    }
}