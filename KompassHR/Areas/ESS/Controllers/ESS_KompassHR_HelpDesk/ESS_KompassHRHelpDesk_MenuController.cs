using Dapper;
using KompassHR.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KompassHR.Areas.ESS.Controllers.ESS_KompassHR_HelpDesk
{
    public class ESS_KompassHRHelpDesk_MenuController : Controller
    {
        SqlConnection sqlcon = new SqlConnection(DapperORM.connectionString);
        DynamicParameters param = new DynamicParameters();
        GetMenuList ClsGetMenuList = new GetMenuList();
        // GET: ESS/ESS_KompassHRHelpDesk_Menu
        public ActionResult ESS_KompassHRHelpDesk_Menu(int? id, int? ScreenId)
        {
            try
            {
                if (Session["EmployeeId"] == null)
                {
                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                // CHECK IF USER HAS ACCESS OR NOT
                int screenId = Request.QueryString["ScreenId"] != null ? Convert.ToInt32(Request.QueryString["ScreenId"]) : 793;
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
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), id, ScreenId, "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }
                else
                {
                    var GetMenuList = ClsGetMenuList.GetMenu(Session["UserAccessPolicyId"].ToString(), Convert.ToInt32(Session["ModuleId"]), Convert.ToInt32(Session["ScreenId"]), "Form", "Transation");
                    ViewBag.GetUserMenuList = GetMenuList;
                }

                DynamicParameters param = new DynamicParameters();
                param.Add("@p_HelpDeskId_Encrypted", "List");
                param.Add("@p_Qry", " and Kompass_HelpDesk.HelpDesk_RequestBy='"+ Session["EmployeeId"] + "' ORDER BY HelpDeskId DESC");
                param.Add("@p_msg", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
                var data = DapperORM.DynamicList("sp_List_Kompass_Helpdesk", param);
                ViewBag.GetList = data;
                string msg = param.Get<string>("@p_msg");

                DynamicParameters param1 = new DynamicParameters();
                param1.Add("@p_Employeeid", Session["EmployeeId"]);               
                var DashboardCount = DapperORM.DynamicList("sp_Dashboard_KompassHR_Helpdesk", param1);
                ViewBag.DashboardCount = DashboardCount;

                return View();
            }
            catch (Exception ex)
            {
                Session["GetErrorMessage"] = ex.Message;
                return RedirectToAction("ErrorPage", "Login");
            }
        }

        public ActionResult GetTicketActionDetails(int ticketId)
        {
            try
            {
                if (Session["EmployeeId"]==null)
                {

                    return RedirectToAction("Login", "Login", new { Area = "" });
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}