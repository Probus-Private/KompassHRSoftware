using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Recruitment
{
    public class Recruitment_OfferLetterGeneration
    {
        public double OfferLetterGenId { get; set; }
        public string OfferLetterGenId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpId { get; set; }
        public double OfferLetterGenBranchId { get; set; }
        public double OfferLetterGenInterviewId { get; set; }
        public double OfferLetterGenResourceId { get; set; }
        public double OfferLetterGenResourceEmployeeId { get; set; }
        public double OfferLetterGenPoolEmployeeId { get; set; }
        public double OfferLetterGenResumeId { get; set; }
        public double OfferLetterGenDepartmentId { get; set; }
        public double OfferLetterGenDesignationId { get; set; }
        public double OfferLetterGenGradeId { get; set; }
        public double OfferLetterGenReportingHRId { get; set; }
        public string CandidateName { get; set; }
        public string EmailId { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }
        public float CTC { get; set; }
        public float GrossAmount { get; set; }
        public float InHandAmount { get; set; }
        public DateTime JoiningDate { get; set; }
        public string OfferLetterTemplate { get; set; }
        public double OfferLetterApproverId { get; set; }
    }
}