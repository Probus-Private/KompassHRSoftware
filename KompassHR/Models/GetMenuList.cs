using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Models
{
    public class GetMenuList
    {
        public List<dynamic> GetMenu(string AccessPolicyId, int? ModuleId, int? ScreenId, string ScreenType, string ScreenMenuType)
        {
            DynamicParameters param1 = new DynamicParameters();
            param1.Add("@p_AccessPolicyId", AccessPolicyId);
            param1.Add("@p_ModuleId", ModuleId);
            param1.Add("@p_ScreenMenuType", ScreenMenuType);
            param1.Add("@p_ScreenType", ScreenType);              
            param1.Add("@p_ScreenSubId", ScreenId);

            var GetMenuList = DapperORM.ExecuteSP<dynamic>("sp_Access_SubMenuList", param1).ToList();
            return GetMenuList;
        }
    }
}