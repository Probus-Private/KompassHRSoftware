using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.MyContact
{
    public class Mas_ContactCategory
    {
        public double ContactCategoryId { get; set; }
        public string ContactCategoryId_Encrypted { get; set; }       
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
       // public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string ContactCategoryName { get; set; }       
    }
}