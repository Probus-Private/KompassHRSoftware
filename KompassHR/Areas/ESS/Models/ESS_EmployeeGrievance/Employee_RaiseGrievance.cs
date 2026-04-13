using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Models.ESS_EmployeeGrievance
{
    public class Employee_RaiseGrievance
    {
        public double RaiseGrievanceId { get; set; }
        public string RaiseGrievanceId_Encrypted { get; set; }
        public double GrievanceSubCategoryId { get; set; }
        public double GrievanceCategoryId { get; set; }
        public string GrievanceCategory { get; set; }
        public string GrievanceSubCategory { get; set; }
        public string SubCatDescription { get; set; }
        public DateTime GrievanceRaiseDate { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        public string GrievanceFilePath { get; set; }

        public bool Deactivate { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string EmployeeName { get; set; }
        public string ApproveRejectRemark { get; set; }
        public double DocId { get; set; }
        public double ApproveRejectBy { get; set; }
        public DateTime ApproveRejectDate { get; set; }


    }
}
