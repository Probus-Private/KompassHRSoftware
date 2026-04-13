using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TMS
{
    public class TaskCategory_ProjectModuleMapping
    {
        public double TaskCate_ProjModuleMappingId { get; set; }
        public string TaskCate_ProjModuleMappingId_Encrypted { get; set; }
        public bool UseBy { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double TaskCategoryId { get; set; }
        public double ProjectId { get; set; }
        public double ModuleId { get; set; }
        public bool IsActive { get; set; }
        public string TaskCategoryName { get; set; }
        public string ProjectName { get; set; }
        public string ModuleName { get; set; }
    }
}