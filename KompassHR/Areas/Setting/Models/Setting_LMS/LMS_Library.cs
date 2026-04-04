using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_LMS
{
    public class LMS_Library
    {
        public double LMSLibraryId { get; set; }
        public string LMSLibraryId_Encrypted { get; set; }
        public string Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string URL { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double LMSLibrary_CategoryId { get; set; }
        public string Description { get; set; }
        public string DocumentPath { get; set; }
        public string FileType { get; set; }
        public bool IsAssessmentRequired { get; set; }
        public bool IsDigitalSignature { get; set; }
        public string Title { get; set; }
    }
}