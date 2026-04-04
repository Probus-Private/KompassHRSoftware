using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{
    public class FNFResignationType
    {
        public double ResignationTypeId { get; set; }
        public string ResignationType_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public string ResignationType { get; set; }
    }
}