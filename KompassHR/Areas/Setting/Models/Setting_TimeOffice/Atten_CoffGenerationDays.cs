using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_CoffGenerationDays
    {
        public double COFFGenerationDaysId { get; set; }
        public string COFFGenerationDaysId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int COFFGenerationDaysBranchId { get; set; }
       // public double COFFGenerationDay { get; set; }
        public List<double> COFFGenerationDay { get; set; }

    }
}