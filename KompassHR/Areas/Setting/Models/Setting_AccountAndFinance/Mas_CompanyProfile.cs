using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_AccountAndFinance
{
    public class Mas_CompanyProfile
    {
        [Key]
        public double CompanyId { get; set; }
        public string CompanyName { get; set; }

    }
}