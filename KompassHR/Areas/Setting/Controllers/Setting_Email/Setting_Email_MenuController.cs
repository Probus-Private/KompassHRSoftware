using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.Setting.Controllers.Setting_Email
{
    public class Setting_Email_MenuController : Controller
    {

        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: Setting/Setting_Email_Menu
        public ActionResult Setting_Email_Menu(int? id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                if (id != null)
                {
                    Session["MenuId"] = id;
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_UserRightsModuleId", Session["MenuId"]);
                param.Add("@p_Transactiontype", "Setting");
                var GetMenuList = DapperORM.ExecuteSP<dynamic>("sp_GetUserRights_MenuList", param).ToList(); // SP_getReportingManager
                ViewBag.GetUserMenuList = GetMenuList;
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