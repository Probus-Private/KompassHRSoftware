using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_RegimeDeclaration
    {
        public int RegimeDeclarationId { get; set; }
        public string RegimeDeclarationId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double FYearId { get; set; }
        public string RegimeType { get; set; }
        public bool Declaration { get; set; }
      
    }
}