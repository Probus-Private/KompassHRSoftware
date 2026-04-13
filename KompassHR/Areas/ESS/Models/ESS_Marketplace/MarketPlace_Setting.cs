using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.ESS.Models.ESS_Marketplace
{
    public class MarketPlace_Setting
    {
        public long MarketPlaceSettingID { get; set; }  

        public string MarketPlaceSettingID_Encrypted { get; set; }  

        public long CmpID { get; set; }  

        public bool Deactivate { get; set; }  

        public string CreatedBy { get; set; }  

        public DateTime? CreatedDate { get; set; }  

        public string ModifiedBy { get; set; }  

        public DateTime? ModifiedDate { get; set; } 

        public string MachineName { get; set; }  

        public string BackDate_Days { get; set; }  

        public string MonthlyLimit { get; set; }  
    }

}