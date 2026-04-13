using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_SaleForce
{
    public class MAS_REPORTMAPPING
    {
        public int Fid { get; set; }
        public Nullable<System.DateTime> Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public string Encrypted_Id { get; set; }
        public string UserEnc { get; set; }
        public string UserName { get; set; }
        public Nullable<int> MAS_REPORT_Fid { get; set; }
        public Nullable<int> USERS_Fid { get; set; }
        public Nullable<bool> Deleted { get; set; }
    }
}