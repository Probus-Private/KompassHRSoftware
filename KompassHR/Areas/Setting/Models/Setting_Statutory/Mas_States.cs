using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Mas_States
    {
        [Key]
        public double StateId { get; set; }
        public string StateName { get; set; }
    }
}