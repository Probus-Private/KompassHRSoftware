using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TMS
{
    public class ClientProjectMapping
    {
        public double ClientProjectMappingId { get; set; }
        public string ClientProjectMappingId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ClientId { get; set; }
        public double ProjectId { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProjectMapping
    {
        public double ClientProjectMappingId { get; set; }
        public double ProjectID { get; set; }
        public bool IsActive { get; set; }
        public String ProjectName { get; set; }
        public double ClientId { get; set; }
    }
}