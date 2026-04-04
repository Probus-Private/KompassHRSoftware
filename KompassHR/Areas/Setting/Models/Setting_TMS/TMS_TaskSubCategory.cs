using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TMS
{
    public class TMS_TaskSubCategory
    {
        public double TaskSubCategoryId { get; set; }
        public string TaskSubCategoryId_Encrypted { get; set; }
        public bool UseBy { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double TaskCategoryId { get; set; }
        public string TaskSubCategoryName { get; set; }
    }
}