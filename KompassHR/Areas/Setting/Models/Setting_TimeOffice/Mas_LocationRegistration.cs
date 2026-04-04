using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_TimeOffice
{
    public class Mas_LocationRegistration
    {
        public double LocationRegistrationId { get; set; }
        public string LocationRegistrationId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string LocationName { get; set; }       
        public string LocationLatitude { get; set; }
        public string LocationLongitude { get; set; }
        public string LocationRemark { get; set; }
        public string LocationAreaRange { get; set; }
    }
}