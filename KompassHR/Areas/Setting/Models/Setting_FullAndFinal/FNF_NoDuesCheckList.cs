using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{
    public class FNF_NoDuesCheckList
    {
        [Key]
        public double FNFNoDuesId { get; set; }
        public string FNFNoDuesId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string NoDuesAndClearenceTitle { get; set; }
        public string NoDuesCheckListName { get; set; }
        public string RequiredClearance { get; set; }
    }
}