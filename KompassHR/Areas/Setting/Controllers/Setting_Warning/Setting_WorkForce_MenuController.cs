using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Warning
{
    public class Setting_WorkForce_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        GetMenuList ClsGetMenuList = new GetMenuList();
        DynamicParameters param = new DynamicParameters();
        // GET: Setting/Setting_WorkForce_Menu
        #region Setting_WorkForce_Menu
        public ActionResult Setting_WorkForce_Menu(int? id, int? ScreenId)
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
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "Form", "Setting");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "Form", "Setting");
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
        #endregion

    }
}