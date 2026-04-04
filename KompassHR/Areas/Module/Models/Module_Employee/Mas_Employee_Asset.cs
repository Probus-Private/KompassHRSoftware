using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Module.Models.Module_Employee
{
    public class Mas_Employee_Asset
    {
        public double AssetId { get; set; }
        public string AssetId_Encrypted { get; set; }
        public double AssetEmployeeId { get; set; }
        public string AssetName { get; set; }
        public DateTime? Date { get; set; }
        public string Remark { get; set; }
        public string AssetRemark { get; set; }
        public bool RequiredAlert { get; set; }
        public DateTime? RequiredAlertDate { get; set; }
        public string RequiredAlertText { get; set; }
        public DateTime? ReturnDate { get; set; }
        public String ReturnRemark { get; set; }
        public bool ReturnStatus { get; set; }
        public string ReturnStatusText { get; set; }
        
        // public DateTime? FromDate { get; set; }
        // public DateTime? ToDate { get; set; }
        public double EmployeeId { get; set; }
        public double CompanyId { get; set; }
        public double BranchId { get; set; }
    }

}