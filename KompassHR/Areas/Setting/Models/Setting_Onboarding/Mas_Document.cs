using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Onboarding
{
    public class Mas_Document
    {
        public double DocumentID { get; set; }
        public string DocumentID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DocumentName { get; set; }
        public bool ProofForBirth { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsBackPage { get; set; }
        public float? FileSizeInKb { get; set; }

    }
}