using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Language
    {
        public double LangEmpID { get; set; }
        public string LangEmpID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double LanguageEmployeeID { get; set; }
        public double LanguageID { get; set; }
        public bool empRead { get; set; }
        public bool empWrite { get; set; }
        public bool empSpeak { get; set; }
       
    }
}