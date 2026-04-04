using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Module_Employee_DocumentAssignment
    {
        public double DocAssignmentMasterId { get; set; }
        public string DocAssignmentMasterId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string AssignmentTitle { get; set; }
        public string Description { get; set; }
        public string Instruction { get; set; }
        public DateTime DueDate { get; set; }
        public double CmpId { get; set; }
        public double BranchId { get; set; }
        public double? DepartmentId { get; set; }
        public double? EmployeeId { get; set; }
        public string DocumentPath { get; set; }
        public string Remark { get; set; }
        public double DocAssignmentDetailsId { get; set; }
    }

    public class DocumentAssginedEmployee
    {
        public double EmployeeId { get; set; }
        public double DocAssignmentDetailsId { get; set; }
        public bool IsActive { get; set; }
    }
}