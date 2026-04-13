using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Training
{
    public class Training_TrainingPlan
    {
        public double TrainingPlan_MasterId { get; set; }
        public string TrainingPlan_MasterId_Encrypted { get; set; }
        public double TrainingPlan_DetailsId { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string MachineName { get; set; }
        public double TrainingCalenderId { get; set; }
        public string TrainingCalenderName { get; set; }
        public string Batch { get; set; }
        public double EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DesignationName { get; set; }
        public string DepartmentName { get; set; }
        public string PrimaryMobile { get; set; }
        public string PersonalEmailId { get; set; }
        public bool IsTrainingConduct { get; set; }


        public string Location { get; set; }
        public string TrainingProviderSource { get; set; }
        public double TrainingProviderId { get; set; }
        public double TrainerId { get; set; }
        public double TrainingAgencyId { get; set; }
        public string DepartmentId { get; set; }
       
    }


    public class TrainingPlan_Document
    {
        public double TrainingPlanDocId { get; set; }
        public string TrainingPlanDoc_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double TrainingPlan_PlanId { get; set; }
        public string Title { get; set; }
        public string DocumentPath { get; set; }
        public string FilteType { get; set; }
        public double LMSLibraryId { get; set; }
        public double TrainingPlanId { get; set; }
    }
}