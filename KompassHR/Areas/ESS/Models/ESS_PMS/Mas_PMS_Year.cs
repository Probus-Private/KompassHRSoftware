using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
        public class Mas_PMS_Year
        {
            public long YearId { get; set; }

            public string YearId_Encrypted { get; set; }

            public bool? Deactivate { get; set; }

            public bool? UseBy { get; set; }

            public string CreatedBy { get; set; }

            public DateTime? CreatedDate { get; set; }

            public string ModifiedBy { get; set; }

            public DateTime? ModifiedDate { get; set; }

            public string MachineName { get; set; }

            public string Year { get; set; }
        }
}