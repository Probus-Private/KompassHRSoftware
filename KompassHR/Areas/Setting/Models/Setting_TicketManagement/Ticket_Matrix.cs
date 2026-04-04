using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Models.Setting_TicketManagement
{
    public class Ticket_Matrix
    {
        public double TicketMatrixID { get; set; }
        public string TicketMatrixID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double TicketMatrixKPICategoryId { get; set; }
        public string TicketType { get; set; }
        public double DepartmentID { get; set; }
        public double TicketMatrixReponsibleEmployeeId { get; set; }
        public string TicketMatrixUnit { get; set; }
        public string TicketMatrixTAT { get; set; }
        public double TicketSubCategoryId { get; set; }

        public double? Level1Employee { get; set; }
        public double? Level2Employee { get; set; }
        public double? Level3Employee { get; set; }
        public double? Level4Employee { get; set; }

        public double? Level1TAT { get; set; }
        public double? Level2TAT { get; set; }
        public double? Level3TAT { get; set; }
        public double? Level4TAT { get; set; }

        public string Level1Status { get; set; }
        public string Level2Status { get; set; }
        public string Level3Status { get; set; }
        public string Level4Status { get; set; }

        public bool chkAllLevel1 { get; set; }
        public bool chkAllLevel2 { get; set; }
        public bool chkAllLevel3 { get; set; } 
        public bool chkAllLevel4 { get; set; }


        public String SeverityLevel { get; set; }
        //public double? ResponseTime { get; set; }
        //public double? ResolutionTime { get; set; }

        public int? Critical_ResponseTime { get; set; }
        public int? Critical_ResolutionTime { get; set; }

        public int? High_ResponseTime { get; set; }
        public int? High_ResolutionTime { get; set; }

        public int? Medium_ResponseTime { get; set; }
        public int? Medium_ResolutionTime { get; set; }

        public int? Low_ResponseTime { get; set; }
        public int? Low_ResolutionTime { get; set; }

        [AllowHtml]
        public string KeyPoints { get; set; }

    }


}