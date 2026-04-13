using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Prime
{
    public class Mas_CompanyDocumentLiabrary
    {
        public double CompanyDocumentLiabraryId { get; set; }
        public string CompanyDocumentLiabraryId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string URL { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double CompanyDocumentLiabraryCategoryId { get; set; }
        public string Description { get; set; }
        public string DocumentPath { get; set; }
    }
}