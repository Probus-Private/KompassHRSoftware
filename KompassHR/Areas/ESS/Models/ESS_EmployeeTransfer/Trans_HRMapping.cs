using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_EmployeeTransfer
{
    public class Trans_HRMapping
    {
        public double FNFHRMappingID { get; set; }
        public string FNFHRMappingID_Encrypted { get; set; }
        public double CompanyID { get; set; }
        public double BusinessUnitID { get; set; }
        public double EmployeeID { get; set; }
        public double FNFNoDuesId { get; set; }
    }
}