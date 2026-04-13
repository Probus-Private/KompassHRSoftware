using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Assets
{
    public class Mas_EmployeeInfo
    {
        public string EmployeeNo { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public DateTime? JoiningDate { get; set; }
    }
}