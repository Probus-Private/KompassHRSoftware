using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{
    public class FNF_GeneralSetting
    {
        [Key]
        public double FNFSettingId { get; set; }
        public string FNFSettingId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool IsImmediatelyShow { get; set; }
        public bool IsNoticePeriodPurchaseShow { get; set; }
        public int BackDateEntryDays { get; set;}
        public int FutureDateEntryDays { get; set; }
       
    }
}