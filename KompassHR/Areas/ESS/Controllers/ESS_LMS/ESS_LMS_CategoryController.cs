using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.LMS
{
    public class ESS_LMS_CategoryController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_LMS_Category
        #region ESS_LMS_Category
        public ActionResult ESS_LMS_Category(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }

                // var GetGroupId = DapperORM.QuerySingle($@"Select CategoryGroupMasterId from LMS_Category_GroupAssign where Deactivate=0 And EmployeeId={Session["EmployeeId"]}");
                var GetGroupIds = DapperORM.DynamicQueryList($@"Select CategoryGroupMasterId  from LMS_Category_GroupAssign where Deactivate=0 And EmployeeId={Session["EmployeeId"]}");
                
                List<dynamic> finalMenuList = new List<dynamic>();

                if (GetGroupIds != null && GetGroupIds.Count > 0)
                {
                    foreach (var item in GetGroupIds)
                    {
                        //if (GetGroupId != null)
                        //{
                        DynamicParameters param = new DynamicParameters();
                        param.Add("@p_CompanyId", Session["CompanyId"]);
                        param.Add("@p_BranchId", Session["BranchId"]);
                        param.Add("@p_EmployeeId", Session["EmployeeId"]);
                        param.Add("@p_CategoryGroupId", item.CategoryGroupMasterId);
                        // ViewBag.GetUserMenuList = DapperORM.ExecuteSP<dynamic>("sp_LMS_ESS_Category", param).ToList();
                        var menu = DapperORM.ExecuteSP<dynamic>("sp_LMS_ESS_Category", param).ToList();
                        finalMenuList.AddRange(menu);
                    }
                    ViewBag.GetUserMenuList = finalMenuList;
                }
                else
                {
                    ViewBag.GetUserMenuList = new List<dynamic>();
                }

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
        #endregion
    }
}