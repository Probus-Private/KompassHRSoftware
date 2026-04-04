using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Prime
{
    public class Mas_Designation
    {
       
        public double DesignationId { get; set; }
        public string DesignationId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string  CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string DesignationName { get; set; }
        public string DesignationSACcode { get; set; }
        public bool DesignationTypePayroll { get; set; }
        public bool DesignationTypeBilling { get; set; }
        public bool DesignationUseBy { get; set; }
         public bool RecruitmentApplicable { get; set; }
        public double? MinimumAge { get; set; }
        public double? MaximumAge { get; set; }
        public double? MinimumExperience { get; set; }
        public double? MaximumExperience { get; set; }
        public double? MinimumBudget { get; set; }
        public double? MaximumBudget { get; set; }
        public bool ApplicableForWorker { get; set; }
    }
}