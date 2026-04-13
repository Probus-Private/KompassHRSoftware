using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Models.ESS_Training
{
    public class Training_TrainingCalender
    {
        public double TrainingCalenderId { get; set; }
        public string TrainingCalenderId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string TrainingCalenderName { get; set; }
        [AllowHtml]
        public string TrainingDescription { get; set; }
        public double TraningCategoryId { get; set; }
        public string TrainingCategory { get; set; }
        public string TraningSubCategoryId { get; set; }
        public string TrainingSubcategory { get; set; }
        public string SelectedTrainingSubcategories { get; set; }

        public string DepartmentNames { get; set; }
        public DateTime MonthYear { get; set; }
          public string DepartmentId { get; set; }
        //   public string EndTime { get; set; }
        public string SelectedDepartments { get; set; }
        public string ModeOfTraining { get; set; }
        public string Location { get; set; }
        public string GPSLattitude { get; set; }
        public string GPSLongitude { get; set; }

        public string TrainingProviderSource { get; set; }
        public double TrainingProviderId { get; set; }
        public string TrainingProvidedBy { get; set; }

        public double MaxParticipants { get; set; }
        public double MinParticipants { get; set; }
        public double EnrolledCount { get; set; }

        public string AssesmentRequired { get; set; }
        public double AssesmentTypeId { get; set; }

        public string WaitList { get; set; }

        public string TrainerId { get; set; }
        public double EmployeeId { get; set; }
        public double TrainingAgencyId { get; set; }
        public double TrainingTypeId { get; set; }
        public string TrainingTypeName { get; set; }
        public double TrainingfrequencyId { get; set; }
        public string Trainingfrequency { get; set; }

        public string IsEnrollmentRequired { get; set; }
        public DateTime EnrollmentLastDate{ get; set; }
        public string TrainingFeesType { get; set; }
        public string TrainingFeesPaidType { get; set; }
        public double TrainingFeesAmount { get; set; }
        public string EnrollmentAttachment { get; set; }

        public bool CourseMaterial { get; set; }
        public bool FoodIncluded { get; set; }
        public bool Accommodation { get; set; }
        public bool Travel { get; set; }
        public bool Certificate { get; set; }
        public string BannerFile { get; set; }

        public List<AllDropDownBind> InternalEmployees { get; set; }
        public List<AllDropDownBind> TrainingSubcategories { get; set; }
    }
}