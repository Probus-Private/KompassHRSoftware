using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_LMS
{
    public class LMSLibrary_Document
    {
        public double LMSLibraryDocId { get; set; }
        public string LMSLibraryDoc_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double LMS_Library_LMSLibraryId { get; set; }
        public string Title { get; set; }
        public string DocumentPath { get; set; }
        public string FilteType { get; set; }
        public double LMSLibraryId { get; set; }
    }
}