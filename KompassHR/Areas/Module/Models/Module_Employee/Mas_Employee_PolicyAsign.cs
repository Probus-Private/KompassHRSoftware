using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_PolicyAsign
    {
        public double EmployeePolicyId { get; set; }
        public string EmployeePolicyId_Encrypted { get; set; }
        public double EmployeePolicyEmployeeID { get; set; }
        public double PolicyId { get; set; }
        public string GroupName { get; set; }

    }
}