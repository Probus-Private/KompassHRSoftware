using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Statutory
{
    public class Payroll_ESICCode
    {
        public double ESICCodeId { get; set; }
        public string ESICCodeId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double CmpID { get; set; }
        public double ESICCodeBranchId { get; set; }
        public string ESICCode { get; set; }
        public string ESICAddress { get; set; }
        public string ESICRemark { get; set; }
    }
}