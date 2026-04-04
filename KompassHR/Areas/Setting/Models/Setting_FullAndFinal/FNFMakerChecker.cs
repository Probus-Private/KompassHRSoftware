using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{
    public class FNFMakerChecker
    {
        public double MakerCheckerId { get; set; }
        public string MakerChecker_Encrypted { get; set; }
        public bool Deactivate { get; set; }
        public bool UseBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public string MachineName { get; set; }
        public double MakerCheckerCmpId { get; set; }
        public double MakerCheckerBranchId { get; set; }
        //public double TopManagerId { get; set; }
        public string MakerEmpNames { get; set; }
        public DateTime MonthYear { get; set; }
        public string FNFMakerEmpId { get; set; }
        //   public string EndTime { get; set; }
        public string SelectedFNFMakerEmpIds { get; set; }
      //  public double FNFMakerEmpId { get; set; }
        public double FNFCheckerEmpId { get; set; }
        public double AccountEmpId { get; set; }
        public double OtherEmpId { get; set; }
        //public bool IsTopManagerApprove { get; set; }
    }
}