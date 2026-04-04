using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Recruitment
{
    public class Recruitment_Agency
    {
        public double AgencyId { get; set; }
        public string AgencyId_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string AgencyName { get; set; }
        public string ContactPerson { get; set; }
        public string EmailAddress { get; set; }
        public string MobileNo { get; set; }
    }
}