using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_UserAccessPolicy
{
    public class Tool_UserAccessPolicyDetails
    {
      //  public List<UserAccessRecoardList> UserAccessRecoardList { get; set; }

        public int UserGroupDetailsID { get; set; }
        public int UserGroupDetails_UserGroupID { get; set; }
        public int UserGroupDetails_ScreenID { get; set; }
        public int UserGroupDetails_ModuleID { get; set; }
        public int SrNo { get; set; }
        public bool IsMenu { get; set; }
        public bool IsSave { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }
        public bool IsList { get; set; }
    }

    //public class UserAccessRecoardList
    //{
    //    public double UserGroupDetails_UserGroupID { get; set; }
    //    public double ScreenId { get; set; }
    //    public bool IsMenu { get; set; }
    //    public bool IsSave { get; set; }
    //    public bool IsUpdate { get; set; }
    //    public bool IsDelete { get; set; }
    //    public bool IsList { get; set; }
    //}
    //public class Tool_UserAccessPolicyDetails
    //{
    //    public List<UserAccessRecoardList> UserAccessRecoardList { get; set; }
    //}
}