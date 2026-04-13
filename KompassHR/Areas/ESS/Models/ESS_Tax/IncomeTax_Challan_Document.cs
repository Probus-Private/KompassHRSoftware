using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Tax
{
    public class IncomeTax_Challan_Document
    {

        public int ChallanDocumentId { get; set; }
        public string ChallanDocument_EncryptedId { get; set; }
        public bool Deactivate { get; set; }
        public int IncomeTax_Challan_ChallanId { get; set; }
        public string Title { get; set; }
        public string DocumentPath { get; set; }
        public string FileType { get; set; }
    }
}