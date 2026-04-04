using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_OwnObjectiveCreation
    {
        public decimal OwnObjectiveCreationId { get; set; }
        public string OwnObjectiveCreationId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DepartmentId { get; set; }
        public string EmployeeId { get; set; } // Single EmployeeId for stored procedure
        public List<int> EmployeeIds { get; set; } // Collection for multiple employee IDs    
        public string AlignWith { get; set; }
        public int ObjectiveId { get; set; }
        public string Origin { get; set; }
        public string KRA { get; set; }
        public string SmartPrinciple { get; set; }
        public string UOM { get; set; }
        public string Target { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Weightage { get; set; }
        public string Visibility { get; set; }
        public string Description { get; set; }
        public string Attachment { get; set; }
        public string KPICategory { get; set; }
        public bool IsAlign { get; set; }
        public string TargetType { get; set; }
        public int KPINameId { get; set; }
        public double ActualValue { get; set; }
        public bool IsFinalSubmit { get; set; }
        public double PMS_YearId { get; set; }
        public string AlignWithType { get; set; }
        public bool IsStandardKRA { get; set; }
        public bool StandardKRAValue { get; set; }
        public int? CompanyObjectiveCreationId { get; set; }

    }
}