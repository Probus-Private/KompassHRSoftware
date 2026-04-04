using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_LMS
{
    public class ESS_LMS_TrainingMenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_LMS_TrainingMenu
        public ActionResult ESS_LMS_TrainingMenu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 446;
                bool CheckAccess = new BulkAccessClass().CheckAccess(screenId, Convert.ToInt32(Session["UserAccessPolicyId"]));
                if (!CheckAccess)
                {
                    Session["AccessCheck"] = "False";
                    return RedirectToAction("Dashboard", "Dashboard", new { area = "" });
                }
                if (ScreenId != null)
                {
                    Session["ModuleId"] = id;
                    Session["ScreenId"] = ScreenId;
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "SubForm", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "SubForm", "Transation");
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