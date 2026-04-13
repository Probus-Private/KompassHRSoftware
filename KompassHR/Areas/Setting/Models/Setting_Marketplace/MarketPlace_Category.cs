using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_Marketplace
{
    public class MarketPlace_Category
    {
        public double MarketPlaceCategoryID { get; set; }
        public string MarketPlaceCategoryID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public string MarketPlaceCategory { get; set; }
    }
}