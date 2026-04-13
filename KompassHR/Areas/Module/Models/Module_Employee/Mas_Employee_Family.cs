using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Family
    {
        public double FamilyId { get; set; }
        public string FamilyId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int FamilyEmployeeId { get; set; }
        public string MemberName { get; set; }
        public string Relation { get; set; }
        public string Age { get; set; }
        public string DOB { get; set; }
        public string AadharNo { get; set; }
        public bool ESIC_InsuranceType { get; set; }
        public string Member_Residing { get; set; }
        public string TownName { get; set; }
        public string StateName { get; set; }
    }
}