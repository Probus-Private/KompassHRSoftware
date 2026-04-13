using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Dashboards.Models
{
    public class ESSDashboard
    {
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string DesignationName { get; set; }
        public string JoiningDate { get; set; }
        public string PrimaryMobile { get; set; }
        public string CompanyMobileNo { get; set; }
        public string PersonalEmailId { get; set; }
        public string Gender { get; set; }
        public string Experience { get; set; }
        public string PhotoPath { get; set; }
    }

    public class HighLowPerformerVM
    {
        public string PerformanceType { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }

        public decimal TotalFinalRating { get; set; }
        public decimal KRAFinalRating { get; set; }
        public decimal CompetencyFinalRating { get; set; }
    }

}