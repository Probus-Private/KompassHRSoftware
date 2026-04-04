using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Prime
{
    public class Mas_CompanyDocument
    {
      
        public double CompanyDocumentID { get; set; }
        public string CompanyDocumentID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double DocumentBranchID { get; set; }
        public string DocumentName { get; set; }
        public string fname { get; set; }
        public string FileType { get; set; }
       // public string fcontent { get; set; }
        public string FilePath { get; set; }

    }
}