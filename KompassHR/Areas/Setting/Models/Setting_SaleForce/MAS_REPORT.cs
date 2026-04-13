using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_SaleForce
{
    public class MAS_REPORT
    {
        public int Fid { get; set; }
        public Nullable<System.DateTime> Fdate { get; set; }
        public string Mid { get; set; }
        public string Mip { get; set; }
        public string Encrypted_Id { get; set; }
        public Nullable<int> BusinessNameId { get; set; }
        public string Module { get; set; }
        public string ReportName { get; set; }
        public string ViewName { get; set; }
        public string Description { get; set; }
        public Nullable<int> Set_Module_Fid { get; set; }
        public Nullable<bool> Deleted { get; set; }
        public string SubModule { get; set; }
        public string RptMapEncId { get; set; }
        
    }
}
