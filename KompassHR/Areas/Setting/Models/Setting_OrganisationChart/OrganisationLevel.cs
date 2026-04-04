using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_OrganisationChart
{
    public class OrganisationLevel
    {
        public double OrganisationChartId { get; set; }
        public string OrganisationChartId_Encrypted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public bool Deactivate { get; set; }
        public double OrganisationTypeId { get; set; }
        public double OrganisationLevelId { get; set; }
        public string OrganisationRemark { get; set; }
        public double OrganisationSubLevelId { get; set; }
        public string OrganisationTypeName { get; set; }
        public string Level { get; set; }
        public string SubLevel { get; set; }
        public string OrganisationTitle { get; set; }
        


    }
}
