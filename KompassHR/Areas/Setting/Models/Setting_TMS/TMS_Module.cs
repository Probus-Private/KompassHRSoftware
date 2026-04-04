using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TMS
{
    public class TMS_Module
    {
        public double ModuleId { get; set; }
        public string ModuleId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ModuleName { get; set; }
        public bool IsActive { get; set; }
    }
}