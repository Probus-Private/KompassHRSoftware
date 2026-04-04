using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_TimeOffice
{
    public class Mas_LocationRegistrationMapping_Master
    {
        public int LocationRegistrationMappingIMasterId { get; set; }
        public string LocationRegistrationMappingIMasterId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string GroupName { get; set; }
        public int LocationRegistrationId { get; set; }
    }
}