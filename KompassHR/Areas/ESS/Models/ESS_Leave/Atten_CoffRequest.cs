using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Leave
{
    public class Atten_CoffRequest
    {
        public double CoffRequestID { get; set; }
        public string CoffRequestID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double CoffRequestBranchID { get; set; }
        public bool Deactivate { get; set; }
        public string MachineName { get; set; }
        public int DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public double CoffRequesteEmployeeID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Reason { get; set; }
        public double AttenCoffBalanceID { get; set; }
        public double CoffRequestGenratetionId { get; set; }
        public string CoffRequestDayType { get; set; }
        public float TotalDays { get; set; }
        public string RequestFrom { get; set; }
        public DateTime CoffGenerationDate { get; set; }
        public double CoffShiftId { get; set; }
    }

    public class Coff_GeneratedID
    {
        public int CoffGeneratedID { get; set; }
        public float Days { get; set; }
    }
}