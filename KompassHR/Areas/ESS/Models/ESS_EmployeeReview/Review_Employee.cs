using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_EmployeeReview
{
    public class Review_Employee
    {
        public double EmployeeReviewId { get; set; }
        public string EmployeeReviewId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ReviewerEmployeeId { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public double ReviewEmployeeId { get; set; }
        public string ReviewTittle { get; set; }
        public string Description { get; set; }
        public string Suggestion { get; set; }
        public string Rating { get; set; }
        public string ReviewPanel { get; set; }
        public DateTime NextReviewDate { get; set; }
    }
}