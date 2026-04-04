using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_TeamObjectiveCreation
    {
        public long TeamObjectiveCreationId { get; set; }
        public string TeamObjectiveCreationId_Encrypted { get; set; }
        public long DepartmentId { get; set; }
        public long EmployeeId { get; set; } // Single EmployeeId for stored procedure
        public List<int> EmployeeIds { get; set; } // Collection for multiple employee IDs 
        public bool IsAlign { get; set; }

        public string AlignWith { get; set; }
        public long ObjectiveId { get; set; }
        public string KRA { get; set; }
        public string SmartPrinciple { get; set; }
        public string UOM { get; set; }
        public string Target { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string KPI { get; set; }
        public int Weightage { get; set; }
        public string Visibility { get; set; }
        public string Description { get; set; }
        public string Attachment { get; set; }
        public string KPICategory { get; set; }

        public int KPINameId { get; set; }
    }
}