using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace KompassHR.Areas.Setting.Models.Setting_Prime
{
    public class Mas_Currency
    {
        [Key]
        public double CurrencyId { get; set; }
        public string CurrencyId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CurrencyCountryId { get; set; }
        public string CurrencyName { get; set; }
        public string ShortName { get; set; }
        public string CountryName { get; set; }
        public string CurrencyNotation { get; set; }

    }
}