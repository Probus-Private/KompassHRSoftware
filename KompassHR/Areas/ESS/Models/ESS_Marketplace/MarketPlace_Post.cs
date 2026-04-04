using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Marketplace
{
    public class MarketPlace_Post
    {
        public double MarketPlacePostID { get; set; }
        public string MarketPlacePostID_Encrypted { get; set; }
        public double CmpID { get; set; }
        public double BranchID { get; set; }
        public bool Deactivate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { set; get; }
        public string MachineName { set; get; }
        public string ModifiedDate { set; get; }
        public string DocNo { set; get; }
        public double MarketPlaceCategoryID { get; set; }
        public string MarketPlacePostDesciption { set; get; }
        public string MarketPlacePostTitle { set; get; }
        public string MarketPlacePostPrice { get; set; }
        public string ContactNo { get; set; }
        public string Photo1 { get; set; }
        public string Photo2 { get; set; }
        public string Photo3 { get; set; }
        public bool IsActive { get; set; }
        public string BuyerName { get; set; }
        public DateTime BuyerDate { get; set; }
        public DateTime DocDate { get; set; }
    }


    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}