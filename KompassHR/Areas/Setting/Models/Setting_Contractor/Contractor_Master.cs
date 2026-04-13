using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Contractor
{
    public class Contractor_Master
    {

        public double ContractorId { get; set; }
        public string ContractorId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public String ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string ContractorName { get; set; }
        public string ContractorAddress { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ContactNo { get; set; }
        public string WhatsAppNo { get; set; }
        public string ContactorEmailID { get; set; }
        public string FaxNo { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonEmailID { get; set; }
        public string ContactPersonContactNo { get; set; }
        public double CmpId { get; set; }
        public double ContractorBranchID { get; set; }

    }
}