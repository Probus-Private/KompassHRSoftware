using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_SaleForce
{
    public class MAS_REPORLEVEL
    {
        public int Fid { get; set; }
        public DateTime Fdate { get; set; }
        public int ReportingLever { get; set; }
        public string ReportingRole { get; set; }

        public bool Deleted { get; set; }
    }
}