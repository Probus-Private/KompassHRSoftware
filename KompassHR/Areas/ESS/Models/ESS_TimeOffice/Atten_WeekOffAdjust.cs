using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class Atten_WeekOffAdjust
    {
        public double WeekOffAdjustId { get; set; }
        public string WeekOffAdjustId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int? CmpId { get; set; }
        public int? WOBranchId { get; set; }
        public int? WODepartmentID { get; set; }
        public int? WOAdjustEmployeeId { get; set; }
        public DateTime WOFromDate { get; set; }
        public DateTime WOAdjustDate { get; set; }
        public DateTime DocDate { get; set; }
        public int? SubUnitId { get; set; }
        public int? ContractorID { get; set; }
        public string WOAdjustType { get; set; }
        public class WeekOffAdjust
        {
            public double WOAdjustEmployeeId { get; set; }
            //public string WeekOffAdjustId_Encrypted { get; set; }
        }

        public class WeekOffAdjust_List
        {
            public DateTime? WOFromDate { get; set; }
            public DateTime? WOAdjustDate { get; set; }
            public int? CmpId { get; set; }
            public int? WOBranchId { get; set; }
            public int? EmployeeID { get; set; }
            public string WeekOffAdjustId_Encrypted { get; set; }
        }
    }
}