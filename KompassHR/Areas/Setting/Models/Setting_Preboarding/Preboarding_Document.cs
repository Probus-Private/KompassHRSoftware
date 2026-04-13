using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Preboarding
{
    public class Preboarding_Document
    {
        public long PreboardingDocumentID { get; set; }

        public string PreboardingDocumentId_Encrypted { get; set; }

        public bool Deactivate { get; set; }

        public bool UseBy { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string MachineName { get; set; }

        public string DocumentName { get; set; }

        public string DocumentPath { get; set; }
    }
}