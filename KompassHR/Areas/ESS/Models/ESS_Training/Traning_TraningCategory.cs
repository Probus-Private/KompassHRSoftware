using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Training
{
    public class Traning_TraningCategory
    {
        public double TrainingCategoryId { get; set; }
        public string TrainingCategoryId_Encrypted { get; set; }
        public bool IsActive { get; set; } = true;
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string TrainingCategory { get; set; }
    }
}