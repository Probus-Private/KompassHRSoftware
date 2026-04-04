using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Reference
    {
        public double ReferenceId { get; set; }
        public string ReferenceId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ReferenceEmployeeId { get; set; }
        public string PrimaryNeighboursName { get; set; }
        public string PrimaryNeighboursMobile { get; set; }
        public string SecondaryNeighboursName { get; set; }
        public string SecondaryNeighboursMobile { get; set; }
        public string RelativeName { get; set; }
        public string RelativeRelation { get; set; }
        public string RelativeMobile { get; set; }
        public string PrimaryEmergencyName { get; set; }
        public string PrimaryEmergencyRelation { get; set; }
        public string PrimaryEmergencyMobile { get; set; }
        public string SecondaryEmergencyName { get; set; }
        public string SecondaryEmergencyRelation { get; set; }
        public string SecondaryEmergencyMobile { get; set; }
        public string PrimaryCompanyRefName { get; set; }
        public string PrimaryCompanyRefMobile { get; set; }
        public string SecondaryCompanyRefName { get; set; }
        public string SecondaryCompanyRefMobile { get; set; }      

    }
}