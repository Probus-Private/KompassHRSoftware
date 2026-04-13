using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Training
{
    public class TrainingSubcategory_Document
    {
        public double TrainingSubCategoryDocId { get; set; }
        public string TrainingSubCategoryDocId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double Training_TrainingCategory_TrainingSubCategoryId { get; set; }
        public string Title { get; set; }
        public string DocumentPath { get; set; }
        public string FilteType { get; set; }
        public double TrainingSubCategoryId { get; set; }
    }
}