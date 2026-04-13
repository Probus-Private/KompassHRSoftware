using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_VMS
{
    public class Visitor_Report
    {
        [Display (Name="Company Name")]
        public int? CmpID { get; set; }
        public int? BranchId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}