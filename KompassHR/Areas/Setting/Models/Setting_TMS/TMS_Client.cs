using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TMS
{
    public class TMS_Client
    {
        public double ClientID { get; set; }
        public string ClientID_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string ClientName { get; set; }
        public bool IsActive { get; set; } = true;
        //  public string ResponsilePerson { get; set; }
        public int? ClientDetailsId { get; set; }
        public string ResponsiblePerson { get; set; }
        public List<TMS_ClientDetails> ClientDetails { get; set; } = new List<TMS_ClientDetails>();
    }

    public class TMS_ClientDetails
    {
        public int? ClientDetailsId { get; set; }
        public string ClientDetailsId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double ClientId { get; set; }
        public string ResponsiblePerson { get; set; }

    }

}