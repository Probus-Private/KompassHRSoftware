using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{
    public class FNF_Reason
    {
        [Key]
        public double ReasonID { get; set; }
        public string ReasonID_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public string ReasonName { get; set; }
        public bool UseBy { get; set; }
    }
}