using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_OrganisationChart
{
    public class Organisation_Type
    {

        public double OrganisationTypeId { get; set; }
        public string OrganisationTypeId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public String OrganisationTypeName { get; set; }
        public String OrganisationTypeRemark { get; set; }
        public bool ShowForEmployee { get; set; }
    }
}