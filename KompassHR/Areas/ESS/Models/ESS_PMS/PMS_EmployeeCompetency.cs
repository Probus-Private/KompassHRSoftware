using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_EmployeeCompetency
    {
        public double EmployeeCompetencyId { get; set; }
        public string EmployeeCompetencyId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CompetencyMappingId { get; set; }
        public double EmployeeId { get; set; }
        public double SelfRating { get; set; }
        public string Remark { get; set; }
        public double ManagerRating { get; set; }
        public string ManagerRemark { get; set; }
        public DateTime ManagerRatingDate { get; set; }
        public string Status { get; set; }
        public double ApproveRejectBy { get; set; }
        public string ApproveRejectRemark { get; set; }
        public DateTime ApproveRejectDate { get; set; }
        public string RequestStatus { get; set; }
        public double CurrentLevel { get; set; }
    }
}