using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_AdminDashboard
{
    public class ESS_AdminDashboard_DashboardController : Controller
    {
        // GET: ESS/ESS_AdminDashboard_Dashboard
        public ActionResult ESS_AdminDashboard_Dashboard()
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                DynamicParameters paramMenu = new DynamicParameters();
                paramMenu.Add("@p_AccessPolicyId", Session["UserAccessPolicyId"]);
                paramMenu.Add("@p_ScreenMenuType", "Setting");
                var GerSettingMenu = DapperORM.ExecuteSP<dynamic>("sp_Access_GetSideMenuList", paramMenu).ToList();
                Session["GetSettingMenulist"] = GerSettingMenu;
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