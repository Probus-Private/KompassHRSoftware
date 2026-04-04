using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_FullAndFinal
{

    public class FNF_TermsAndCondition
    {
        public double FNFTermsAndConditionID { get; set; }
        public string FNFTermsAndConditionID_Encrypted { get; set; }
        public double CompanyID { get; set; }
        public double BusinessUnitID { get; set; }
        public string TermsAndConditionName { get; set; }
    }
}