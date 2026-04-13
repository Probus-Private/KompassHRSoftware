using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_KPIMaster
    {
        public int KPIId { get; set; }

        public string KPIId_Encrypted { get; set; }

        public bool? Deactivate { get; set; }

        public bool? UseBy { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string MachineName { get; set; }

        public string Category { get; set; }

        public string KPIName { get; set; }

        public string Description { get; set; }

        

        public bool? Status { get; set; }

        public DateTime? LastStatusDate { get; set; }

        public int? LastStutusBy { get; set; } // Assuming typo in

    }
}