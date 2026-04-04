using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Onboarding
{
    public class Mas_EmployeeSeries
    {
        public double EmployeeSeriesId { get; set; }
        public string EmployeeSeriesId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double EmployeeSeriesEmployeeId { get; set; }
        public string EmployeeSeriesName { get; set; }
        public string EmployeeSeriesPrefix { get; set; }
        public string EmployeeSeriesSuffix { get; set; }
    }
}