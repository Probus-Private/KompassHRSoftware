using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Training
{
    public class Training_TrainingAgency
    {
        public double TrainingAgencyId { get; set; }
        public string TrainingAgencyId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string TrainingAgencyName { get; set; }
        public string MobileNo { get; set; }
        public string WhatappsNo { get; set; }
        public string EmailId { get; set; }
        public string Specialization { get; set; }
        public string ContactPersonName { get; set; }
        public string TotalExperience { get; set; }
        public string NoOfTrainingConducted { get; set; }
        public string ContractDuration { get; set; }
        public string PhotoPath { get; set; }
        public string LinkedinProfile { get; set; }
        public DateTime FromMonth { get; set; }
        public DateTime ToMonth { get; set; }
    }
}