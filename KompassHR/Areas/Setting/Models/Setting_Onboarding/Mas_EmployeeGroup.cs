using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Onboarding
{
    public class Mas_EmployeeGroup
    {
        public double EmployeeGroupId { get; set; }
        public string EmployeeGroupId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string EmployeeGroupName { get; set; }

    }
}