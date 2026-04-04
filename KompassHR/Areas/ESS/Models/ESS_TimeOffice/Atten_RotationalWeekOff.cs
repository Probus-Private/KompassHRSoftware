using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class Atten_RotationalWeekOff
    {
        public double RotationalWeekOffId { get; set; }
        public string RotationalWeekOffId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double RotationalWeekOffBranchId { get; set; }
        public double RotationalWeekOffFinancialYaerId { get; set; }
        public double RotationalWeekOffDepartmentID { get; set; }
        public double RotationalWeekOffEmployeeId { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime RotationalWeekOffMonthYear { get; set; }
        public DateTime RotationalWeekOffDate { get; set; }

    }

    public class RotationalWeekOff
    {
        public string EmployeeId { get; set; }
    }

}