using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Atten_CoffSetting
    {
        public double? CoffID { get; set; }
        public string CoffID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string CoffTypeName { get; set; }
        public bool CoffRegularDay { get; set; }
        public float? CoffRegularCoff0_5Day_Min { get; set; }
        public float? CoffRegularCoff0_5Day_Max { get; set; }
        public float? CoffRegularCoff1Day_Min { get; set; }
        public float? CoffRegularCoff1Day_Max { get; set; }
        public float? CoffRegularCoff1_5Day_Min { get; set; }
        public float? CoffRegularCoff1_5Day_Max { get; set; }
        public float? CoffRegularCoff2Day_Min { get; set; }
        public float? CoffRegularCoff2Day_Max { get; set; }
        public int? CoffWOPHCoff0_5day_Min { get; set; }
        public int? CoffWOPHCoff0_5day_Max { get; set; }
        public int? CoffWOPHCoff1day_Min { get; set; }
        public int? CoffWOPHCoff1day_Max { get; set; }
        public int? CoffWOPHCoff1_5day_Min { get; set; }
        public int? CoffWOPHCoff1_5day_Max { get; set; }
        public int? CoffWOPHCoff2day_Min { get; set; }
        public int? CoffWOPHCoff2day_Max { get; set; }
        public int? CoffVaild { get; set; }
      
    }
}