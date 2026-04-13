using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy
{
    public class UserAccess_PolicyScreenMapping
    {
        public double ScreenMasterId { get; set; }
        public string ScreenDisplayMenuName { get; set; }
        public string ModuleName { get; set; }
        public string ScreenMenuType { get; set; }
        public string DescriptionForUser { get; set; }
    }
}