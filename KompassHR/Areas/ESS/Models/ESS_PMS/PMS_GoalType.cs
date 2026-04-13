using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_PMS
{
    public class PMS_GoalType
    {
        public long GoalTypeID { get; set; } // Unchecked (likely identity or primary key)

        public string GoalTypeID_Encrypted { get; set; }

        public long CmpID { get; set; }

        public bool Deactivate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string MachineName { get; set; }

        public string GoalType { get; set; }

        public string Description { get; set; }
    }
}