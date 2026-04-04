using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Reports.Controllers.Reports_AllReport
{
    public class ESS_Reports_AllReport_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: Reports/ESS_Reports_AllReport_Menu
        public ActionResult ESS_Reports_AllReport_Menu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    DynamicParameters param = new DynamicParameters();
                    param.Add("@p_AccessPolicyId", Session["UserAccessPolicyId"]);
                    //param.Add("@p_ModuleId", ModuleId);
                    param.Add("@p_ScreenMenuType", "Transation");
                    param.Add("@p_ScreenType", "Form");
                    param.Add("@p_ScreenSubId", Session["ScreenId"]);
                    var GetMenuList = DapperORM.ExecuteSP<dynamic>("sp_Access_AllReportsMenuList", param).ToList();
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    DynamicParameters param1 = new DynamicParameters();
                    param1.Add("@p_AccessPolicyId", Session["UserAccessPolicyId"]);
                    //param1.Add("@p_ModuleId", ModuleId);
                    param1.Add("@p_ScreenMenuType", "Transation");
                    param1.Add("@p_ScreenType", "Form");
                    param1.Add("@p_ScreenSubId", Session["ScreenId"]);
                    var GetMenuList = DapperORM.ExecuteSP<dynamic>("sp_Access_AllReportsMenuList", param1).ToList();
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }
    }
}