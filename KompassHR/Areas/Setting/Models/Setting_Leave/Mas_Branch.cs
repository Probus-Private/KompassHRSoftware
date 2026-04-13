using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Leave
{
    public class Mas_Branch
    {
        [Key]
        public double BranchId { get; set; }
        public string BranchName { get; set; }
       
    }
}