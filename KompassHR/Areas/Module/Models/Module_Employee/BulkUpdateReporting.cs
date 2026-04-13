using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class BulkUpdateReporting
    {
        public int? CmpId { get; set; }
        public int? BranchId { get; set; }
        public int? ManagerId { get; set; }
        public int? ModelId { get; set; }
        public int? LevelId { get; set; }
        public int? NewManagerId { get; set; }
        public int? EmployeeId { get; set; }
    }
}