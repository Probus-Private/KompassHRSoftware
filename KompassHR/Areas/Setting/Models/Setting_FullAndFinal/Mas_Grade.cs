using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{
    public class Mas_Grade
    {
        [Key]
        public double GradeId { get; set; }
        public string GradeId_Encrypted { get; set; }
        public string GradeName { get; set; }
        public string Description { get; set; }
        
    }
}