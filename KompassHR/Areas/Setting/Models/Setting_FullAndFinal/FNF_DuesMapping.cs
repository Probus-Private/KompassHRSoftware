using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{
    
    public class FNF_DuesMapping
    {
        public double FNFNoDuesMappingID { get; set; }
        public string FNFNoDuesMappingID_Encrypted { get; set; }
        public double CompanyID { get; set; }
        public double BusinessUnitID { get; set; }
        public double EmployeeID { get; set; }
        public double FNFNoDuesId { get; set; }
    }
}