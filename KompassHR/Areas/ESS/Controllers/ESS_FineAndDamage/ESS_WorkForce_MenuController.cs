using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_FineAndDamage
{
    public class ESS_WorkForce_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        // GET: ESS/ESS_WorkForce_Menu
        public ActionResult ESS_WorkForce_Menu(int? id)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { area = "" });
                }
                if(id != null)
                {
                    Session["ModuleId"] = id;
                }
                param.Add("@p_EmployeeId", Session["EmployeeId"]);
                param.Add("@p_UserRightsModuleId", Session["ModuleId"]);
                param.Add("@p_Transactiontype", "Transation");
                var GetMenuList = DapperORM.ExecuteSP<dynamic>("sp_GetUserRights_MenuList", param).ToList(); // SP_getReportingManager
                ViewBag.GetUserMenuList = GetMenuList;
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}