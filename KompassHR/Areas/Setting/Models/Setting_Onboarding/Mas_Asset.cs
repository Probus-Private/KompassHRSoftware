using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Onboarding
{
    public class Mas_Asset
    {
        [Key]
        public double AssetId { get; set; }
        public string AssetId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public string AssetName { get; set; }
        public string AssetType { get; set; }
        public string Remark { get; set; }
        public bool UseBy { get; set; }
    }
}