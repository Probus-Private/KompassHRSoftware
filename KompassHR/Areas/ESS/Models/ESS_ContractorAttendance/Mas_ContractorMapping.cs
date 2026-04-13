using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_ContractorAttendance
{
    public class Mas_ContractorMapping
    {
        public double ContractorMapId { get; set; }
        public string ContractorMapId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ContractorID { get; set; }
        public double BranchID { get; set; }
        public double CmpID { get; set; }
        public bool IsActive { get; set; }
    }

    public class ListContractorMapping
    {
        public double ContractorID { get; set; }
    }

}