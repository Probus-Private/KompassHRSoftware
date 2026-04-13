using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_SaleForce
{
    public class MAS_SERVICE
    {
        public int? Fid { get; set; }
        public string Encrypted_Id { get; set; }
        public DateTime Fdate { get; set; }
        public int MAS_SERCATG { get; set; }
        public string Service { get; set; }
        public string ServiceCategory { get; set; }
        public string Attachment { get; set; }
    }
}