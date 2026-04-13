using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Travel
{
    public class Travel_Documents
    {
        public int TravelDocsID { get; set; }
        public string TravelDocsID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public int CmpId { get; set; }
        public int TravelBranchID { get; set; }
        public int DocNo { get; set; }
        public int TravelCalenderID { get; set;}
        public int TravelPlansID { get; set; }
        public int TravelPlanEmployeeId { get; set;}
        public string PlanType { get; set; }
        public string Mode { get; set; }
        public string DocumentName { get; set; }
        public string FileLocation { get; set; }
        public string Note { get; set; }
        
    }
}