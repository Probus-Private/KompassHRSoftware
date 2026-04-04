using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_SaleForce
{
    public class MAS_STAGES
    {
        public double Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public int UserId { get; set; }
        public string Stage { get; set; }
        public string Origin { get; set; }
        public string Level { get; set; }
        public bool Deleted { get; set; }
    }
}